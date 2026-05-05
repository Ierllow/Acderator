using Intense.Master;
using R3;
using UnityEngine;

namespace Song
{
    public enum ETutorialEventType { ShowIntro, ShowStep, ShowComplete }

    public class TutorialEvent
    {
        public readonly ETutorialEventType Type;
        public readonly object Data;

        TutorialEvent(ETutorialEventType type, object data = default)
        {
            Type = type;
            Data = data;
        }

        public static TutorialEvent ShowIntro(TutorialMaster tutorial) => new(ETutorialEventType.ShowIntro, tutorial);
        public static TutorialEvent ShowStep(TutorialStepMaster step) => new(ETutorialEventType.ShowStep, step);
        public static TutorialEvent ShowComplete() => new(ETutorialEventType.ShowComplete);
    }

    public class SongTutorialStateController : ITutorialController
    {
        private ETutorialState cachedTutorialState;
        private TutorialData tutorialData;
        private float tutorialElapsedTime;
        private bool isRunning;

        public bool IsCompleted => tutorialData.IsCompleted;

        public readonly Subject<TutorialEvent> TutorialEventSubject = new();

        public void UpdateState(ETutorialState currentState) => cachedTutorialState = currentState;

        public void Init(TutorialData tutorialData)
        {
            this.tutorialData = tutorialData;
            UpdateState(ETutorialState.Intro);
        }

        public void ChangeState(bool isPlaying) => isRunning = isPlaying;

        public void Tick()
        {
            if (!isRunning) return;

            tutorialElapsedTime += Time.deltaTime;

            switch (cachedTutorialState)
            {
                case ETutorialState.Intro:
                    UpdateState(ETutorialState.Step);
                    break;
                case ETutorialState.Step:
                    TickStep();
                    break;
                default:
                    break;
            }
        }

        public void CompleteCurrentStep()
        {
            tutorialData.CompleteCurrentStep();
            if (tutorialData.IsCompleted)
            {
                UpdateState(ETutorialState.Complete);
                ShowComplete();
                return;
            }
            UpdateState(ETutorialState.Step);
        }

        private void TickStep()
        {
            var currentStep = tutorialData.GetCurrentStep();
            if (currentStep == default)
            {
                UpdateState(ETutorialState.Complete);
                ShowComplete();
                return;
            }

            if (tutorialElapsedTime < currentStep.TriggerTime) return;

            ShowCurrentStep();
            UpdateState(ETutorialState.None);
        }

        public void ShowIntro() => TutorialEventSubject.OnNext(TutorialEvent.ShowIntro(tutorialData.TutorialMaster));
        private void ShowCurrentStep() => TutorialEventSubject.OnNext(TutorialEvent.ShowStep(tutorialData.GetCurrentStep()));
        private void ShowComplete() => TutorialEventSubject.OnNext(TutorialEvent.ShowComplete());
    }
}
