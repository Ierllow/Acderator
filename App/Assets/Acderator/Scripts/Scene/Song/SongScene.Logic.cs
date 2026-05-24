using Cysharp.Threading.Tasks;
using Intense;
using Intense.Api;
using Intense.UI;

namespace Song
{
    public partial class SongScene
    {
        private async UniTask LoadAssets() 
        {
            if (!sceneContext.IsRestart)
            {
                await songAssetLoader.LoadBundles(sceneManager.CurrentSceneType, destroyCancellationToken).AddWatcherTo(failFastExceptionWatcher);
            }
            await (sceneContext.IsRestart ? sceneManager.FadeInAsync().AddWatcherTo(failFastExceptionWatcher) : UniTask.WhenAll(backTelopLayerController.ShowSongIntro(sceneContext.SongInfo)), sceneManager.FadeInAsync());
            songLayerController.Show(sceneContext.IsAuto);
            await notesLineController.Show().AddWatcherTo(failFastExceptionWatcher);
            backTelopLayerController.SetBackgroundImage(sceneContext.SongInfo.Bg);
            songControllerResolver.Loop.UpdateState(ESongState.Ready);
        }

        private async UniTask OnReadySong()
        {
            var loadResult = await songAssetLoader.LoadChart().AddWatcherTo(failFastExceptionWatcher);
            if (loadResult.IsSuccess)
            {
                songManagerResolver.Notes.Init(loadResult.ChartInfo);
                songManagerResolver.Factory.Init();
                var offset = songManagerResolver.Notes.GetSpawnOffset(notesLineController.LaneLength);
                songControllerResolver.Loop.SetOffset(offset);
                songControllerResolver.Spawner.Init(loadResult.ChartInfo.NoteDataList, offset);
                songLayerController.Init(sceneContext.SongInfo.Sid);
                await backTelopLayerController.FadeIn();
                songControllerResolver.TutorialState?.ShowIntro();
                return;

            }
            await songPopupLayerController.DetectedError(new ScoreLoadError(loadResult.LoadResult));
        }

        private void OnPlayingSong()
        {
            if (songManagerResolver.Sound.SongExPlayer.GetTime().ToSeconds() > 0)
            {
                songManagerResolver.Sound.PauseSong(false);
                songControllerResolver.Finger?.TrySetUseTouch(!sceneContext.IsAuto);
                return;
            }
            songManagerResolver.Sound.PlaySong(sceneContext.SongInfo.Group);
        }

        private void OnStopSong()
        {
            songManagerResolver.Sound.PauseSong(true);
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
            if (sceneContext.SongMode == ESongMode.Tutorial)
            {
                PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompleted, true);
                PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompletedNeedsFullDownload, true);
                await sceneManager.ChangeSceneAsync(ESceneType.Title);
                return;
            }
            if (sceneContext.SongMode != ESongMode.Normal)
            {
                await scoreSaveError;
                return;
            }

            var currentScore = songLayerController.CurrentScore;
            var noteCount = songManagerResolver.Notes.LoadedChartInfo.NoteCount;
            await frontTelopLayerController.ShowResult(songLayerController.GetSongResult(noteCount));

            var request = new ScoreSubmitRequest { SessionId = sceneContext.SessionId, Score = currentScore };
            var response = await songManagerResolver.Network.RequestAsync(request);
            var isSaved = response?.IsSuccess ?? false;
            if (!isSaved)
            {
                await scoreSaveError;
                return;
            }
            await changeScene;
        }

        private void UpdateSongProgressNext()
        {
            songControllerResolver.Finger?.UpdatePointerInput();
            songControllerResolver.Loop.Tick(songManagerResolver.Sound.SongExPlayer.GetTime().ToSeconds());
            songControllerResolver.TutorialState?.ChangeState(songControllerResolver.Loop.CurrentState == ESongState.Playing);
            songControllerResolver.TutorialState?.Tick();
            songControllerResolver.Tutorial?.AdvanceByTapIfPossible(songControllerResolver.Loop.CurrentState != ESongState.Playing);
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

        private void FingerSubscribeNext(FingerInfo fingerInfo, int lane)
        {
            if (lane >= 0) notesLineController.SetLaneLightActive(lane, fingerInfo.FingerType);
            songLayerController.UpdateSongLayer(fingerInfo);
#if UNITY_EDITOR
            songLayerController.DebugInfoView.UpdateDebugInfo(fingerInfo.NoteBase.NoteData.NoteType, songManagerResolver.Notes.GetDiffSec(fingerInfo.FingerType, fingerInfo.NoteBase.NoteData), songManagerResolver.Notes.CurrentBeat, songLayerController.JudgeCountDict);
#endif
            songControllerResolver.Particle.UpdateParticles(fingerInfo);
            if (fingerInfo.IsMiss)
            {
                frontTelopLayerController.ShowMissMask();
                return;
            }
            songManagerResolver.Sound.PlaySe(fingerInfo.IsFlick ? ESeType.Flick : ESeType.Tap);
        }

        private void OnSongLoopNext(float sec)
        {
            songManagerResolver.Notes.UpdateNoteSpeed();
            songManagerResolver.Notes.UpdateBeat(sec);
            songControllerResolver.Spawner.UpdateSpawn(sec);
            songControllerResolver.Optimizer.UpdatePositionNotes((x) => songControllerResolver.Auto?.NotifyFinger(x));
            songControllerResolver.Auto?.OnAutoFinger();
            if (songManagerResolver.Sound.SongExPlayer.IsPlayEnd()) songControllerResolver.Loop.UpdateState(ESongState.End);
        }

        private async UniTask ConfirmSkipTutorial()
        {
            var isPlaying = songControllerResolver.Loop.CurrentState == ESongState.Playing;
            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            if (await songPopupLayerController.OpenTutorialSkipConfirm())
            {
                songControllerResolver.Tutorial.CompleteTutorial();
                return;
            }
            if (isPlaying) songControllerResolver.Loop.UpdateState(ESongState.Playing);
        }
    }
}