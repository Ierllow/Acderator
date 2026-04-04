using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
using Intense.Attribute;
using Intense.Data;
using Intense.UI;
using System;
using UnityEngine;
using Zenject;
using Intense.Master;

namespace Song
{
    [SceneType(ESceneType.Song)]
    public partial class SongScene : SceneBase
    {

        [Inject] private MasterDataManager masterDataManager;
        [Inject] private AssetBundleManager assetBundleManager;
        [Inject] private SoundManager soundManager;
        [Inject] private ScoreManager scoreManager;
        [RequiredField, SerializeField] private SongLayerController songLayerController;
        [RequiredField, SerializeField] private FrontTelopLayerController frontTelopLayerController;
        [RequiredField, SerializeField] private BackTelopLayerController backTelopLayerController;
        [RequiredField, SerializeField] private SongPopupLayerController songPopupLayerController;
        [RequiredField, SerializeField] private NotesLineController notesLineController;
        [RequiredField, SerializeField] private FailFastExceptionWatcher failFastExceptionWatcher;

        [InjectOptional] private SongApplicationPauseHandler songApplicationPauseHandler;
        [Inject] private NotesManager notesManager;
        [Inject] private NoteFactory noteFactory;
        [Inject] private SongControllerResolver songControllerResolver;
        [Inject] private SongSceneContext sceneContext;

        protected override void Awake()
        {
            failFastExceptionWatcher.Init(destroyCancellationToken);
            base.Awake();
        }

        protected override void Start()
        {
            StartSubscribes();
            base.Start();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (songApplicationPauseHandler?.IsHandlePause(pauseStatus) == false) return;
            songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto);
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            if (!sceneContext.IsRestart)
            {
                sceneContext.SongBundlePathList.ForEach(assetBundleManager.AddLoadAssets);
                await assetBundleManager.LoadAssetsAsync(destroyCancellationToken).AddWatcherTo(failFastExceptionWatcher);
            }
            await (sceneContext.IsRestart ? sceneManager.FadeInAsync().AddWatcherTo(failFastExceptionWatcher) : backTelopLayerController.ShowSongIntro(sceneContext.SongInfo));
            songLayerController.Show(sceneContext.IsAuto);
            await notesLineController.Show().AddWatcherTo(failFastExceptionWatcher);
            backTelopLayerController.SetBackgroundImage(sceneContext.SongInfo.Bg);
            songControllerResolver.Loop.UpdateState(ESongState.Ready);
        });

        protected override async UniTask OnErrorScene()
        {
            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            await songPopupLayerController.DetectedError(new AlertError());
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
            var chart = await assetBundleManager.GetLoadedObjectAsync(sceneContext.SongChartBundlePath).AddWatcherTo(failFastExceptionWatcher);
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
                    songLayerController.Init(sceneContext.SongInfo.Sid);
                    await backTelopLayerController.FadeIn();
                    return;
                }
            }
            await songPopupLayerController.DetectedError(new ScoreLoadError(loadedChartInfo.LoadResult));
        }

        private void OnPlayingSong()
        {
            if (soundManager.SongExPlayer.GetTime().ToSeconds() > 0)
            {
                soundManager.PauseSong(false);
                songControllerResolver.Finger?.TrySetUseTouch(!sceneContext.IsAuto);
                return;
            }
            soundManager.PlaySong(sceneContext.SongInfo.Group);
        }

        private void OnStopSong()
        {
            soundManager.PauseSong(true);
            songControllerResolver.Finger?.TrySetUseTouch(false);
        }

        private async UniTask OnEndSong()
        {
            songLayerController.SetPauseButtonGrayOut(true);
            songControllerResolver.Finger?.TrySetUseTouch(false);

            var changeScene = UniTask.Defer(async () => sceneManager.ChangeSceneAsync(ESceneType.Result, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict)));
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

            var isSaved = await scoreManager.RequestUpdateScoreAsync(sceneContext.SessionId, currentScore);
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
                    await sceneManager.ChangeSceneAsync(ESceneType.Song, sceneContext, true);
                    break;
                case EPopupTapKind.Quit:
                    await sceneManager.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext(masterDataManager));
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
            soundManager.PlaySe(fingerInfo.IsFlick ? ESeType.Flick : ESeType.Tap);
        }

        private void OnSongLoopNext(float sec)
        {
            notesManager.UpdateNoteSpeed();
            notesManager.UpdateBeat(sec);
            songControllerResolver.Spawner.UpdateSpawn(sec);
            songControllerResolver.Optimizer.UpdatePositionNotes((x) => songControllerResolver.Auto?.NotifyFinger(x));
            songControllerResolver.Auto?.OnAutoFinger();
            if (soundManager.SongExPlayer.IsPlayEnd()) songControllerResolver.Loop.UpdateState(ESongState.End);
        }
    }
}