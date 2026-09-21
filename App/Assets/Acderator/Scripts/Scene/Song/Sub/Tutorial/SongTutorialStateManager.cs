#nullable enable

using Intense.Master;
using R3;
using UnityEngine;

namespace Song
{
    public enum TutorialEventType { ShowIntro, ShowStep, ShowComplete }

    public class TutorialEvent
    {
        public readonly TutorialEventType Type;
        public readonly object? Data;

        TutorialEvent(TutorialEventType type, object? data = null)
        {
            Type = type;
            Data = data;
        }

        public static TutorialEvent ShowIntro(TutorialMaster tutorial) => new(TutorialEventType.ShowIntro, tutorial);
        public static TutorialEvent ShowStep(TutorialStepMaster step) => new(TutorialEventType.ShowStep, step);
        public static TutorialEvent ShowComplete() => new(TutorialEventType.ShowComplete);
    }

    public class SongTutorialStateManager
    {
        private TutorialState cachedTutorialState = TutorialState.Intro;
        private float tutorialElapsedTime;
        private bool isRunning;

        private readonly TutorialData tutorialData;

        public bool IsCompleted => tutorialData.IsCompleted;

        private readonly Subject<TutorialEvent> tutorialEventSubject = new();
        public Observable<TutorialEvent> TutorialEventAsObservable => tutorialEventSubject;

        public SongTutorialStateManager(TutorialData tutorialData) => this.tutorialData = tutorialData;

        public void UpdateState(TutorialState currentState) => cachedTutorialState = currentState;

        public void ChangeState(bool isPlaying) => isRunning = isPlaying;

        public void Tick()
        {
            if (!isRunning) return;

            tutorialElapsedTime += Time.deltaTime;

            switch (cachedTutorialState)
            {
                case TutorialState.Intro:
                    UpdateState(TutorialState.Step);
                    break;
                case TutorialState.Step:
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
                UpdateState(TutorialState.Complete);
                ShowComplete();
                return;
            }
            UpdateState(TutorialState.Step);
        }

        private void TickStep()
        {
            var currentStep = tutorialData.GetCurrentStep();
            if (currentStep == default)
            {
                UpdateState(TutorialState.Complete);
                ShowComplete();
                return;
            }

            if (tutorialElapsedTime < currentStep.TriggerTime) return;

            ShowCurrentStep(currentStep);
            UpdateState(TutorialState.None);
        }

        public void ShowIntro() => tutorialEventSubject.OnNext(TutorialEvent.ShowIntro(tutorialData.TutorialMaster));
        private void ShowCurrentStep(TutorialStepMaster currentStep) => tutorialEventSubject.OnNext(TutorialEvent.ShowStep(currentStep));
        private void ShowComplete() => tutorialEventSubject.OnNext(TutorialEvent.ShowComplete());
    }
}