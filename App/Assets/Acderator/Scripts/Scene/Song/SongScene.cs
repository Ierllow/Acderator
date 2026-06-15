#nullable enable

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Api;
using Intense.Attribute;
using Intense.UI;
using R3;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Song
{
    [SceneType(ESceneType.Song)]
    public class SongScene : SceneBase
    {
        [RequiredField, SerializeField] private SongLayerController songLayerController = default!;
        [RequiredField, SerializeField] private FrontTelopLayerController frontTelopLayerController = default!;
        [RequiredField, SerializeField] private BackTelopLayerController backTelopLayerController = default!;
        [RequiredField, SerializeField] private SongPopupLayerController songPopupLayerController = default!;
        [RequiredField, SerializeField] private NotesLineController notesLineController = default!;

        [Inject] private readonly SongControllerResolver songControllerResolver = default!;
        [Inject] private readonly SongManagerResolver songManagerResolver = default!;
        [Inject] private readonly SongSceneContext sceneContext = default!;
        [Inject] private readonly SongAssetLoader songAssetLoader = default!;
        [Inject] private readonly FailFastExceptionWatcher failFastExceptionWatcher = default!;

        protected override void Awake()
        {
            failFastExceptionWatcher.Init(destroyCancellationToken);
            base.Awake();
        }

        private void Start()
        {
            songControllerResolver.Loop.EveryUpdateSongStateWhere.Subscribe(_ => UpdateSongProgressNext()).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.EverySongStateChanged.SubscribeAwait(ChangeStateNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.SongLoopUpdateAsObservable.Subscribe(OnSongLoopNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Spawner.NoteFactoryAsObservable.Select(x => (songManagerResolver.Factory.SpawnNote(x), x)).Where(x => x.Item1 != null).Subscribe(x => songManagerResolver.Notes.AddAliveNote(x.Item1!, x.x)).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.SongLeadInCompletedAsObservable.Where(_ => !sceneContext.IsTutorial()).Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.Playing)).RegisterTo(destroyCancellationToken);
            songControllerResolver.FingerJudgeRequest.JudgmentAsObservable.Subscribe(FingerSubscribeNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Finger.EveryUseTouchChanged.Subscribe(notesLineController.SetLaneLightActiveAll).RegisterTo(destroyCancellationToken);
            songPopupLayerController.ClosedPausePopupAsAsyncEnumerable.TakeWhile(_ => sceneContext.IsNormal()).SubscribeAwait(ClosedPausePopupSubscribeNext).RegisterTo(destroyCancellationToken);
            songPopupLayerController.EverySceneTypeChanged.Where(s => s.IsResult()).SubscribeAwait(async (s, _) => await sceneManager.ChangeSceneAsync(s, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict))).RegisterTo(destroyCancellationToken);
            songLayerController.OnTapPauseButtonAsObservable.TakeWhile(_ => !songControllerResolver.Loop.IsEnd()).SubscribeAwait(async (_, __) => await OnTapPauseButton()).RegisterTo(destroyCancellationToken);
            songManagerResolver.TutorialState?.TutorialEventAsObservable.Subscribe(TutorialEventSubscribeNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialIntroCompletedSubject.Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.Playing)).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialStepCompletedSubject.Subscribe(_ => TutorialStepCompletedSubscribeNext()).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialCompletedSubject.Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.End)).RegisterTo(destroyCancellationToken);

#if UNITY_EDITOR
            EditorApplication.pauseStateChanged -= OnPauseStateChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
#endif
        }

#if UNITY_EDITOR
        protected override void OnDestroy()
        {
            EditorApplication.pauseStateChanged -= OnPauseStateChanged;
            base.OnDestroy();
        }
#endif

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) return;
            if (!sceneContext.IsNormal()) return;
            if (songControllerResolver.Sound.IsPlayEnd || songManagerResolver.Notes.AliveNoteList.IsAliveNotes()) return;

            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto());
        }

        public override void OnCreateScene() => LoadAssets().Forget();

        public override void OnDeleteScene()
        {
            songControllerResolver.Sound.StopSong();
            base.OnDeleteScene();
        }

        protected override async UniTask OnErrorScene()
        {
            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            if (!sceneManager.IsFadeIn) await sceneManager.FadeInAsync();
            await songPopupLayerController.OnOpenErrorPopup();
        }

        private UniTask LoadIntro() => sceneContext.IsRestart switch
        {
            true => sceneManager.FadeInAsync().AddWatcherTo(failFastExceptionWatcher),
            _ => backTelopLayerController.ShowSongIntro(
                sceneContext.SongInfo,
                songManagerResolver.Addressable.GetSprite(sceneContext.SongInfo.Group.ToString()),
                songManagerResolver.Addressable.GetSprite(string.Format("difficulty_{0}", sceneContext.SongInfo.Difficulty))),
        };

        private async UniTask LoadAssets()
        {
            if (!sceneContext.IsRestart)
            {
                await songAssetLoader.LoadAssetsAsync(sceneManager.CurrentSceneType, destroyCancellationToken).AddWatcherTo(failFastExceptionWatcher);
            }
            await (LoadIntro(), sceneManager.FadeInAsync());
            songLayerController.Show(sceneContext.IsAuto());
            await notesLineController.Show().AddWatcherTo(failFastExceptionWatcher);
            backTelopLayerController.SetBackgroundImage(songManagerResolver.Addressable.GetSprite(sceneContext.SongInfo.Bg.ToString()));
            songControllerResolver.Loop.UpdateState(ESongState.Ready);
        }

        private async UniTask ChangeStateNext(ESongState songState)
        {
            switch (songState)
            {
                case ESongState.Ready: await OnReadySong(); break;
                case ESongState.Playing: OnPlayingSong(); break;
                case ESongState.Stop: OnStopSong(); break;
                case ESongState.End: await OnEndSong(); break;
                default: break;
            }
        }

        private async UniTask OnReadySong()
        {
            var loadResult = await songAssetLoader.LoadChart().AddWatcherTo(failFastExceptionWatcher);
            if (loadResult.IsSuccess)
            {
                songManagerResolver.Notes.Init(loadResult.ChartInfo);
                songManagerResolver.Factory.Init();
                var leadInSec = songManagerResolver.Notes.GetInitialSpawnLeadInSec(notesLineController.LaneLength);
                songControllerResolver.Spawner.Init(notesLineController.LaneLength);
                songLayerController.Init(sceneContext.SongInfo.Sid);
                await backTelopLayerController.FadeIn();
                songControllerResolver.Loop.StartLeadIn(leadInSec);
                songManagerResolver.TutorialState?.ShowIntro();
                return;

            }
            songPopupLayerController.OnOpenScoreErrorPopup(loadResult.LoadResult);
        }

        private void OnPlayingSong()
        {
            if (songControllerResolver.Sound.HasStarted)
            {
                songControllerResolver.Sound.PauseSong(false);
                songControllerResolver.Finger.TrySetUseTouch(true);
                return;
            }
            songControllerResolver.Sound.PlaySong(sceneContext.SongInfo.Group);
        }

        private void OnStopSong()
        {
            songControllerResolver.Sound.PauseSong(true);
            songControllerResolver.Finger.TrySetUseTouch(false);
        }

        private async UniTask OnEndSong()
        {
            songLayerController.SetPauseButtonGrayOut(true);
            songControllerResolver.Finger.TrySetUseTouch(false);
            switch ((sceneContext.IsAuto(), sceneContext.SongMode))
            {
                case (true, _): await ChangeResultScene(); break;
                case (_, ESongMode.Tutorial): await CompleteTutorial(); break;
                case (_, ESongMode.Normal): await EndNormalSong(); break;
                default: songPopupLayerController.OnOpenSaveScoreDataErrorPopup(); break;
            }
        }

        private async UniTask CompleteTutorial()
        {
            PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompleted, true);
            PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompletedNeedsFullDownload, true);
            await sceneManager.ChangeSceneAsync(ESceneType.Title);
        }

        private async UniTask EndNormalSong()
        {
            await frontTelopLayerController.ShowResult(songLayerController.GetSongResult(songManagerResolver.Notes.LoadedChartInfo!.NoteCount));

            var request = new ScoreSubmitRequest { SessionId = sceneContext.SessionId, Score = songLayerController.CurrentScore };
            var response = await songManagerResolver.Network.RequestAsync(request);
            if (response?.IsSuccess ?? false)
            {
                await ChangeResultScene();
                return;
            }
            songPopupLayerController.OnOpenSaveScoreDataErrorPopup();
        }

        private UniTask ChangeResultScene() => sceneManager.ChangeSceneAsync(ESceneType.Result, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict));

        private void UpdateSongProgressNext()
        {
            songControllerResolver.Finger.UpdateInput();
            songControllerResolver.Loop.Tick(songControllerResolver.Sound.TimeSec);
            songManagerResolver.TutorialState?.ChangeState(songControllerResolver.Loop.IsPlaying());
            songManagerResolver.TutorialState?.Tick();
            songControllerResolver.Tutorial?.AdvanceByTapIfPossible(!songControllerResolver.Loop.IsPlaying());
        }

        private void OnSongLoopNext(float sec)
        {
            songManagerResolver.Notes.UpdateBeat(sec);
            songManagerResolver.Notes.UpdateNoteSpeed();
            songControllerResolver.Spawner.UpdateSpawn();
            songControllerResolver.PositionUpdater.UpdatePositions();
            if (songControllerResolver.Loop.IsPlaying()) songControllerResolver.FingerJudgeRequest.Judge(sec);
            if (songControllerResolver.Sound.IsPlayEnd) songControllerResolver.Loop.UpdateState(ESongState.End);
        }

        private void FingerSubscribeNext(FingerInfo fingerInfo)
        {
            if (fingerInfo.Lane >= 0) notesLineController.SetLaneLightActive(fingerInfo.Lane, fingerInfo.FingerType);
            songLayerController.UpdateSongLayer(fingerInfo);

            songControllerResolver.Particle.UpdateParticles(fingerInfo);
            if (fingerInfo.IsMiss)
            {
                frontTelopLayerController.ShowMissMask();
                return;
            }
            songControllerResolver.Sound.PlaySe(fingerInfo.ToSeType());
        }

        private async UniTask OnTapPauseButton()
        {
            switch (sceneContext.SongMode)
            {
                case ESongMode.Normal:
                    songControllerResolver.Loop.UpdateState(ESongState.Stop);
                    songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto());
                    break;
                case ESongMode.Tutorial when !songControllerResolver.Loop.IsPlaying():
                    await ConfirmSkipTutorial();
                    break;
                default:
                    break;
            }
        }

        private async UniTask ClosedPausePopupSubscribeNext(EPopupTapKind popupTapKind)
        {
            switch (popupTapKind)
            {
                case EPopupTapKind.Restart:
                    sceneContext.Refresh(true);
                    await sceneManager.ChangeSceneAsync(ESceneType.Song, sceneContext, true);
                    break;
                case EPopupTapKind.Quit:
                    await sceneManager.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                    break;
                case EPopupTapKind.Resume:
                    await frontTelopLayerController.CountDownStart();
                    songControllerResolver.Loop.UpdateState(ESongState.Playing);
                    break;
                default:
                    break;
            }
        }

        private async UniTask ConfirmSkipTutorial()
        {
            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            if (await songPopupLayerController.OpenTutorialSkipConfirm())
            {
                songControllerResolver.Tutorial?.CompleteTutorial();
                return;
            }
            songControllerResolver.Loop.UpdateState(ESongState.Playing);
        }

        private void TutorialEventSubscribeNext(TutorialEvent tutorialEvent)
        {
            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            songControllerResolver.Tutorial?.UpdateTutorial(tutorialEvent);
        }

        private void TutorialStepCompletedSubscribeNext()
        {
            songManagerResolver.TutorialState?.CompleteCurrentStep();
            songControllerResolver.Loop.UpdateState(ESongState.Playing);
        }

#if UNITY_EDITOR
        private void OnPauseStateChanged(PauseState state)
        {
            if (state == PauseState.Paused)
            {
                songControllerResolver.Loop.UpdateState(ESongState.Stop);
                songPopupLayerController.OnOpenPausePopup(sceneContext.IsAuto());
                return;
            }
            songControllerResolver.Loop.UpdateState(ESongState.Playing);
        }
#endif
    }

    static class SongSceneExtensions
    {
        public static ESeType ToSeType(this FingerInfo info) => info.IsFlick ? ESeType.Flick : ESeType.Tap;
        public static bool IsPlaying(this SongLoopController loop) => loop.CurrentState == ESongState.Playing;
        public static bool IsEnd(this SongLoopController loop) => loop.CurrentState == ESongState.End;
        public static bool IsTutorial(this SongSceneContext context) => context.SongMode == ESongMode.Tutorial;
        public static bool IsNormal(this SongSceneContext context) => context.SongMode == ESongMode.Normal;
        public static bool IsAuto(this SongSceneContext context) => context.SongOption.IsAuto;
        public static bool IsAliveNotes(this IReadOnlyList<NoteBase> notes) => notes.Count > 0;
        public static bool IsResult(this ESceneType type) => type == ESceneType.Result;
    }
}