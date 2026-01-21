using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
using Intense.Data;
using Intense.UI;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Song
{
    public partial class SongScene : SceneBase
    {
        [SerializeField] private SongLayerController songLayerController;
        [SerializeField] private FrontTelopLayerController frontTelopLayerController;
        [SerializeField] private BackTelopLayerController backTelopLayerController;
        [SerializeField] private SongPopupLayerController songPopupLayerController;
        [SerializeField] private NotesLineController notesLineController;
        [SerializeField] private FailFastExceptionWatcher failFastExceptionWatcher;

        [Inject] private NotesManager notesManager;
        [Inject] private NoteFactory noteFactory;
        [Inject] private SongControllerResolver songControllerResolver;
        [Inject] private SongSceneContext sceneContext;

        protected override void Awake()
        {
            failFastExceptionWatcher.Init(CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken));
            base.Awake();
        }

        protected override void Start()
        {
            StartSubscribes();
            base.Start();
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            await LoadAssets().AddWatcherTo(failFastExceptionWatcher);
            await (sceneContext.IsRestart ? SceneManager.Instance.FadeInAsync().AddWatcherTo(failFastExceptionWatcher) : backTelopLayerController.ShowSongIntro(sceneContext.SongInfo));
            songLayerController.Show(sceneContext.IsAuto);
            await notesLineController.Show().AddWatcherTo(failFastExceptionWatcher);
            backTelopLayerController.SetBackgroundImage(sceneContext.SongInfo.Bg);
            songControllerResolver.Loop.UpdateState(ESongState.Ready);
        });

        public new async UniTask OnErrorScene()
        {
            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            await songPopupLayerController.DetectedError(new AlertError());
        }

        private void ApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                songControllerResolver.Loop.UpdateState(ESongState.Stop);
                return;
            }
            songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto);
        }

        private async UniTask LoadAssets()
        {
            if (sceneContext.IsRestart) return;
            sceneContext.SongBundlePathList.ForEach(AssetBundleManager.Instance.AddLoadAssets);
            await AssetBundleManager.Instance.LoadAssetsAsync(destroyCancellationToken);
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
            var loadedChartInfo = new LoadedChartInfo();
            var chart = await AssetBundleManager.Instance.GetLoadedObjectAsync(sceneContext.SongChartBundlePath).AddWatcherTo(failFastExceptionWatcher);
            if (chart is TextAsset textAsset)
            {
                await new ChartLoader().LoadChart(textAsset.text, loadedChartInfo).AddWatcherTo(failFastExceptionWatcher);
                if (loadedChartInfo.LoadResult.EnumEquals(ELoadResult.None))
                {
                    notesManager.Init(loadedChartInfo);
                    noteFactory.Init();
                    var offset = notesManager.GetSpawnOffset(notesLineController.LaneLength);
                    songControllerResolver.Loop.SetOffset(offset);
                    songControllerResolver.Spawner.Init(loadedChartInfo.NoteDataList, offset);
                    songLayerController.InitScore();
                    await backTelopLayerController.FadeIn();
                    return;
                }
            }
            await songPopupLayerController.DetectedError(new ScoreLoadError(loadedChartInfo.LoadResult));
        }

        private void OnPlayingSong()
        {
            if (SoundManager.Instance.SongExPlayer.GetTime().ToSeconds() > 0)
            {
                SoundManager.Instance.PauseSong(false);
                songControllerResolver.Finger?.TrySetUseTouch(!sceneContext.IsAuto);
                return;
            }
            SoundManager.Instance.PlaySong(sceneContext.SongInfo.Group);
        }

        private void OnStopSong()
        {
            SoundManager.Instance.PauseSong(true);
            songControllerResolver.Finger?.TrySetUseTouch(false);
        }

        private async UniTask OnEndSong()
        {
            songLayerController.SetPauseButtonGrayOut(true);
            songControllerResolver.Finger?.TrySetUseTouch(false);

            var changeScene = UniTask.Defer(async () => SceneManager.Instance.ChangeSceneAsync(ESceneType.Result, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict)));
            var scoreSaveError = UniTask.Defer(async () => await songPopupLayerController.DetectedError(new ScoreSaveError()));

            if (sceneContext.IsAuto)
            {
                await changeScene;
                return;
            }
            if (!sceneContext.SongMode.EnumEquals(ESongMode.Normal))
            {
                await scoreSaveError;
                return;
            }

            var currentScore = songLayerController.CurrentScore;
            var noteCount = notesManager.LoadedChartInfo.NoteCount;
            await frontTelopLayerController.ShowResult(songLayerController.GetSongResult(noteCount));

            var isSaved = await ScoreManager.Instance.RequestUpdateScoreAsync(sceneContext.SongInfo.Sid, currentScore);
            if (!isSaved)
            {
                await scoreSaveError;
                return;
            }
            await changeScene;
        }

        private void UpdateSongProgressNext()
        {
            songControllerResolver.Loop.Tick();
            songControllerResolver.TutorialState?.ChangeState(songControllerResolver.Loop.SongStateEnumEquals(ESongState.Playing));
            songControllerResolver.TutorialState?.Tick();
            if (songControllerResolver.TutorialState?.IsCompleted ?? false) songControllerResolver.Loop.UpdateState(ESongState.End);
        }

        private async UniTask ClosedPausePopupSubscribeNext(EPopupTapKind popupTapKind)
        {
            switch (popupTapKind)
            {
                case EPopupTapKind.Restart:
                    sceneContext.Refresh(true);
                    await SceneManager.Instance.ChangeSceneAsync(ESceneType.Song, sceneContext, true);
                    break;
                case EPopupTapKind.Quit:
                    await SceneManager.Instance.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                    break;
                case EPopupTapKind.Resume:
                    await frontTelopLayerController.CountDownStart();
                    songControllerResolver.Loop.UpdateState(ESongState.Playing);
                    break;
                default:
                    break;
            }
        }

        private void FingerSubscribeNext(FingerInfo fingerInfo, int lane)
        {
            if (lane >= 0) notesLineController.SetLaneLightActive(lane, fingerInfo.FingerType);
            songLayerController.UpdateSongLayer(fingerInfo);
#if UNITY_EDITOR
            songLayerController.DebugInfoView.UpdateDebugInfo(fingerInfo.NoteBase.NoteData.NoteType, notesManager.GetDiffSec(fingerInfo.FingerType, fingerInfo.NoteBase.NoteData), notesManager.CurrentBeat, songLayerController.JudgeCountDict);
#endif
            songControllerResolver.Particle.UpdateParticles(fingerInfo);
            if (fingerInfo.IsMiss)
            {
                frontTelopLayerController.ShowMissMask();
                return;
            }
            SoundManager.Instance.PlaySe(fingerInfo.IsFlick ? ESeType.Flick : ESeType.Tap);
        }

        private void OnSongLoop(float sec)
        {
            notesManager.UpdateNoteSpeed();
            notesManager.UpdateBeat(sec);
            songControllerResolver.Spawner.UpdateSpawn(sec);
            songControllerResolver.Optimizer.UpdatePositionNotes(songControllerResolver.Auto.NotifyFinger);
            songControllerResolver.Auto?.OnAutoFinger();
            if (SoundManager.Instance.SongExPlayer.IsPlayEnd()) songControllerResolver.Loop.UpdateState(ESongState.End);
        }
    }
}