using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using R3;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Song
{
    public partial class SongScene
    {
        private void StartSubscribes()
        {
            StartSongLogicSubscribes();
            StartFingerSubscribes();
            StartLayerSubscribes();
            StartPauseSubscribes();
        }

        private void StartSongLogicSubscribes()
        {
            songControllerResolver.Loop.EveryUpdateSongStateWhere.Subscribe(_ => UpdateSongProgressNext()).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.EverySongStateChanged.SubscribeAwait(ChangeStateNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.SongLoopUpdateStream.Subscribe(OnSongLoopNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Spawner.NoteFactoryStream.Select(songManagerResolver.Factory.SpawnNote).Where(x => x != default).Subscribe(songManagerResolver.Notes.AddAliveNote).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.SongSecondsZeroWhere.Skip(1).Where(_ => sceneContext.SongMode != ESongMode.Tutorial).Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.Playing)).RegisterTo(destroyCancellationToken);
        }

        private void StartFingerSubscribes()
        {
            songControllerResolver.Finger.JudgmentStream.Subscribe(FingerSubscribeNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Finger.EveryUseTouchChanged.Subscribe(notesLineController.SetLaneLightActiveAll).RegisterTo(destroyCancellationToken);
        }

        private void StartLayerSubscribes()
        {
            songPopupLayerController.ClosedPausePopupAsAsyncEnumerable.TakeWhile(_ => sceneContext.SongMode == ESongMode.Normal).SubscribeAwait(ClosedPausePopupSubscribeNext).RegisterTo(destroyCancellationToken);
            songPopupLayerController.EverySceneTypeChanged.Where(s => !(s == ESceneType.None && songControllerResolver.Loop.CurrentState == ESongState.End) && s == ESceneType.Result).SubscribeAwait(async (s, _) => await sceneManager.ChangeSceneAsync(s, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict))).RegisterTo(destroyCancellationToken);
            songLayerController.OnTapPauseButtonAsObservable.TakeWhile(_ => songControllerResolver.Loop.CurrentState != ESongState.End).SubscribeAwait(async (_, __) => await OnTapPauseButton()).RegisterTo(destroyCancellationToken);
            songControllerResolver.TutorialState?.TutorialEventStream.Subscribe(TutorialEventSubscribeNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialIntroCompletedSubject.Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.Playing)).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialStepCompletedSubject.Subscribe(_ => TutorialStepCompletedSubscribeNext()).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialCompletedReactiveProperty.Skip(1).Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.End)).RegisterTo(destroyCancellationToken);
        }

        private async UniTask OnTapPauseButton()
        {
            switch (sceneContext.SongMode)
            {
                case ESongMode.Normal:
                    songControllerResolver.Loop.UpdateState(ESongState.Stop);
                    songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto);
                    break;
                case ESongMode.Tutorial when songControllerResolver.Loop.CurrentState != ESongState.Playing:
                    await ConfirmSkipTutorial();
                    break;
            }
        }

        private void TutorialEventSubscribeNext(TutorialEvent tutorialEvent)
        {
            if (tutorialEvent.Type is ETutorialEventType.ShowIntro or ETutorialEventType.ShowStep or ETutorialEventType.ShowComplete) songControllerResolver.Loop.UpdateState(ESongState.Stop);
            songControllerResolver.Tutorial.UpdateTutorial(tutorialEvent);
        }

        private void TutorialStepCompletedSubscribeNext()
        {
            songControllerResolver.TutorialState.CompleteCurrentStep();
            if (!songControllerResolver.TutorialState.IsCompleted) songControllerResolver.Loop.UpdateState(ESongState.Playing);
        }

        private void StartPauseSubscribes()
        {
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

        private void OnPauseStateChanged(PauseState state)
        {
            if (state == PauseState.Paused)
            {
                songControllerResolver.Loop.UpdateState(ESongState.Stop);
                songPopupLayerController.OnOpenPausePopup(sceneContext.IsAuto);
                return;
            }
            songControllerResolver.Loop.UpdateState(ESongState.Playing);
        }
#endif
    }
}