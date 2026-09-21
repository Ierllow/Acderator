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
    [SceneType(SceneType.Song)]
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
            songControllerResolver.Loop.SongLeadInCompletedAsObservable.Where(_ => !sceneContext.IsTutorial()).Subscribe(_ => songControllerResolver.Loop.UpdateState(SongState.Playing)).RegisterTo(destroyCancellationToken);
            songControllerResolver.FingerJudgeRequest.JudgmentAsObservable.Subscribe(FingerSubscribeNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Finger.EveryUseTouchChanged.Subscribe(notesLineController.SetLaneLightActiveAll).RegisterTo(destroyCancellationToken);
            songPopupLayerController.ClosedPausePopupAsAsyncEnumerable.TakeWhile(_ => sceneContext.IsNormal()).SubscribeAwait(ClosedPausePopupSubscribeNext).RegisterTo(destroyCancellationToken);
            songPopupLayerController.EverySceneTypeChanged.Where(s => s.IsSongSelect()).SubscribeAwait(async (_, _) => await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext())).RegisterTo(destroyCancellationToken);
            songLayerController.OnTapPauseButtonAsObservable.TakeWhile(_ => !songControllerResolver.Loop.IsEnd()).SubscribeAwait(async (_, __) => await OnTapPauseButton()).RegisterTo(destroyCancellationToken);
            songManagerResolver.TutorialState?.TutorialEventAsObservable.Subscribe(TutorialEventSubscribeNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialIntroCompletedSubject.Subscribe(_ => songControllerResolver.Loop.UpdateState(SongState.Playing)).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialStepCompletedSubject.Subscribe(_ => TutorialStepCompletedSubscribeNext()).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialCompletedSubject.Subscribe(_ => songControllerResolver.Loop.UpdateState(SongState.End)).RegisterTo(destroyCancellationToken);

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
            if (songControllerResolver.Sound.IsPlayEnd || !songManagerResolver.Notes.AliveNoteList.IsAliveNotes()) return;

            songControllerResolver.Loop.UpdateState(SongState.Stop);
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
            songControllerResolver.Loop.UpdateState(SongState.Stop);
            if (!sceneManager.IsFadeIn) await sceneManager.FadeInAsync();
            await songPopupLayerController.OnOpenErrorPopup();
        }

        private UniTask LoadIntro() => sceneContext.IsRestart switch
        {
            true => sceneManager.FadeInAsync().AddWatcherTo(failFastExceptionWatcher),
            _ => backTelopLayerController.ShowSongIntro(
                sceneContext.SongInfo,
                songManagerResolver.Addressable.GetSprite(sceneContext.SongInfo.Group.ToString()),
                songManagerResolver.Addressable.GetSprite(string.Format("difficulty_{0}", sceneContext.SongInfo.Difficulty))).AddWatcherTo(failFastExceptionWatcher),
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
            songControllerResolver.Loop.UpdateState(SongState.Ready);
        }

        private async UniTask ChangeStateNext(SongState songState)
        {
            switch (songState)
            {
                case SongState.Ready: await OnReadySong(); break;
                case SongState.Playing: OnPlayingSong(); break;
                case SongState.Stop: OnStopSong(); break;
                case SongState.End: await OnEndSong(); break;
                default: break;
            }
        }

        private async UniTask OnReadySong()
        {
            var response = await songManagerResolver.Network.RequestAsync(new ChartRequest { Sid = sceneContext.SongInfo.Sid }).AddWatcherTo(failFastExceptionWatcher);
            var loadedChartInfo = response?.IsSuccess == true ? response.ToLoadedChartInfo() : default;
            if (loadedChartInfo?.NoteDataList.Count > 0)
            {
                songManagerResolver.Notes.Init(loadedChartInfo);
                songManagerResolver.Factory.Init();
                var leadInSec = songManagerResolver.Notes.GetInitialSpawnLeadInSec(notesLineController.LaneLength);
                songControllerResolver.Spawner.Init(notesLineController.LaneLength);
                songLayerController.Init(sceneContext.SongInfo.Sid);
                await backTelopLayerController.FadeIn();
                songControllerResolver.Loop.StartLeadIn(leadInSec);
                songManagerResolver.TutorialState?.ShowIntro();
                return;

            }
            songPopupLayerController.OnOpenScoreErrorPopup(isLoadFailure: loadedChartInfo == default);
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
                case (true, _):
                    await ChangeResultScene();
                    break;
                case (_, SongMode.Tutorial):
                    await CompleteTutorial();
                    break;
                case (_, SongMode.Normal):
                    await EndNormalSong();
                    break;
                default:
                    await songPopupLayerController.OpenSaveScoreDataErrorPopup();
                    await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                    break;
            }
        }

        private async UniTask CompleteTutorial()
        {
            PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompleted, true);
            PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompletedNeedsFullDownload, true);
            await sceneManager.ChangeSceneAsync(SceneType.Title);
        }

        private async UniTask EndNormalSong()
        {
            await frontTelopLayerController.ShowResult(songLayerController.GetSongResult(songManagerResolver.Notes.LoadedChartInfo!.NoteCount));
            while (true)
            {
                var request = new ScoreSubmitRequest { SessionId = sceneContext.SessionId, Score = songLayerController.CurrentScore };
                var response = await songManagerResolver.Network.RequestAsync(request);
                if (response?.IsSuccess ?? false)
                {
                    await ChangeResultScene();
                    return;
                }
                if (!await songPopupLayerController.OpenSaveScoreDataErrorPopup())
                {
                    await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                    return;
                }
            }
        }

        private UniTask ChangeResultScene() => sceneManager.ChangeSceneAsync(SceneType.Result, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict));

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
            if (songControllerResolver.Sound.IsPlayEnd) songControllerResolver.Loop.UpdateState(SongState.End);
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
                case SongMode.Normal:
                    songControllerResolver.Loop.UpdateState(SongState.Stop);
                    songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto());
                    break;
                case SongMode.Tutorial when !songControllerResolver.Loop.IsPlaying():
                    await ConfirmSkipTutorial();
                    break;
                default:
                    break;
            }
        }

        private async UniTask ClosedPausePopupSubscribeNext(PopupTapKind popupTapKind)
        {
            switch (popupTapKind)
            {
                case PopupTapKind.Restart:
                    sceneContext.Refresh(true);
                    await sceneManager.ChangeSceneAsync(SceneType.Song, sceneContext, true);
                    break;
                case PopupTapKind.Quit:
                    await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                    break;
                case PopupTapKind.Resume:
                    await frontTelopLayerController.CountDownStart();
                    songControllerResolver.Loop.UpdateState(SongState.Playing);
                    break;
                default:
                    break;
            }
        }

        private async UniTask ConfirmSkipTutorial()
        {
            songControllerResolver.Loop.UpdateState(SongState.Stop);
            if (await songPopupLayerController.OpenTutorialSkipPopup())
            {
                songControllerResolver.Tutorial?.CompleteTutorial();
                return;
            }
            songControllerResolver.Loop.UpdateState(SongState.Playing);
        }

        private void TutorialEventSubscribeNext(TutorialEvent tutorialEvent)
        {
            songControllerResolver.Loop.UpdateState(SongState.Stop);
            songControllerResolver.Tutorial?.UpdateTutorial(tutorialEvent);
        }

        private void TutorialStepCompletedSubscribeNext()
        {
            songManagerResolver.TutorialState?.CompleteCurrentStep();
            songControllerResolver.Loop.UpdateState(SongState.Playing);
        }

#if UNITY_EDITOR
        private void OnPauseStateChanged(PauseState state)
        {
            if (state == PauseState.Paused)
            {
                songControllerResolver.Loop.UpdateState(SongState.Stop);
                songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto());
                return;
            }
            songControllerResolver.Loop.UpdateState(SongState.Playing);
        }
#endif
    }

    static class SongSceneExtensions
    {
        public static SeType ToSeType(this FingerInfo info) => info.IsFlick ? SeType.Flick : SeType.Tap;
        public static bool IsPlaying(this SongLoopController loop) => loop.CurrentState == SongState.Playing;
        public static bool IsEnd(this SongLoopController loop) => loop.CurrentState == SongState.End;
        public static bool IsTutorial(this SongSceneContext context) => context.SongMode == SongMode.Tutorial;
        public static bool IsNormal(this SongSceneContext context) => context.SongMode == SongMode.Normal;
        public static bool IsAuto(this SongSceneContext context) => context.SongOption.IsAuto;
        public static bool IsAliveNotes(this IReadOnlyList<NoteBase> notes) => notes.Count > 0;
        public static bool IsSongSelect(this SceneType type) => type == SceneType.SongSelect;
    }
}