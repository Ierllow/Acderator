using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Master;
using R3;
using R3.Triggers;
using System;
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
            songControllerResolver.Loop.SongLoopUpdateSubject.Subscribe(OnSongLoopNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Spawner.NoteFactorySubject.Select(noteFactory.SpawnNote).Where(x => x != default).Subscribe(notesManager.AddAliveNote).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.SongSecondsZeroWhere.Skip(1).Where(_ => sceneContext.SongMode != ESongMode.Tutorial).Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.Playing)).RegisterTo(destroyCancellationToken);
        }

        private void StartFingerSubscribes()
        {
            songControllerResolver.Auto?.FingerInfoSubject.Subscribe(x => FingerSubscribeNext(x, -1)).RegisterTo(destroyCancellationToken);
            songControllerResolver.Finger?.EveryUseTouchChanged.Subscribe(notesLineController.SetLaneLightActiveAll).RegisterTo(destroyCancellationToken);
            songControllerResolver.Finger?.FingerInfoSubject.TakeWhile(_ => !sceneContext.IsAuto).Subscribe((x) => FingerSubscribeNext(x.Item1, x.Item2)).RegisterTo(destroyCancellationToken);
        }

        private void StartLayerSubscribes()
        {
            void PauseButtonSubscribeCallback()
            {
                songControllerResolver.Loop.UpdateState(ESongState.Stop);
                songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto);
            }
            songPopupLayerController.ClosedPausePopupAsAsyncEnumerable.TakeWhile(_ => sceneContext.SongMode == ESongMode.Normal).SubscribeAwait(ClosedPausePopupSubscribeNext).RegisterTo(destroyCancellationToken);
            songPopupLayerController.EverySceneTypeChanged.Where(s => !(s == ESceneType.None && songControllerResolver.Loop.CurrentState == ESongState.End) && s == ESceneType.Result).SubscribeAwait(async (s, _) => await sceneManager.ChangeSceneAsync(s, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict))).RegisterTo(destroyCancellationToken);
            songLayerController.OnTapPauseButtonAsObservable.TakeWhile(_ => sceneContext.SongMode == ESongMode.Normal && songControllerResolver.Loop.CurrentState != ESongState.End).Subscribe(_ => PauseButtonSubscribeCallback()).RegisterTo(destroyCancellationToken);
            songLayerController.OnTapPauseButtonAsObservable.TakeWhile(_ => sceneContext.SongMode == ESongMode.Tutorial && songControllerResolver.Loop.CurrentState != ESongState.End).Where(_ => songControllerResolver.Loop.CurrentState != ESongState.Playing).SubscribeAwait(async (_, _) => await ConfirmSkipTutorial()).RegisterTo(destroyCancellationToken);
            songLayerController.Subscribes(_ => !sceneContext.IsAuto, destroyCancellationToken);
            songControllerResolver.TutorialState?.TutorialEventSubject.Subscribe(TutorialEventSubscribeNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialIntroCompletedSubject.Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.Playing)).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialStepCompletedSubject.Subscribe(_ => TutorialStepCompletedSubscribeNext()).RegisterTo(destroyCancellationToken);
            songControllerResolver.Tutorial?.TutorialCompletedReactiveProperty.Skip(1).Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.End)).RegisterTo(destroyCancellationToken);
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
            this.OnPauseStateChangedAsObservable().Subscribe((state) =>
            {
                if (state == PauseState.Paused)
                {
                    songControllerResolver.Loop.UpdateState(ESongState.Stop);
                    songPopupLayerController.OnOpenPausePopup(sceneContext.IsAuto);
                    return;
                }
                songControllerResolver.Loop.UpdateState(ESongState.Playing);
            }).RegisterTo(destroyCancellationToken);
#endif
        }
    }
}
