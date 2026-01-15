using Intense.Master;
using R3;
using System;
using UnityEngine;

namespace Song
{
    public enum ETutorialEventType { ShowIntro, ShowStep, ShowComplete, Hide, UpdateProgress }

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
        public static TutorialEvent ShowStep((TutorialStepMaster, bool) step) => new(ETutorialEventType.ShowStep, step);
        public static TutorialEvent ShowComplete() => new(ETutorialEventType.ShowComplete);
        public static TutorialEvent Hide() => new(ETutorialEventType.Hide);
        public static TutorialEvent UpdateProgress((int, int) progress) => new(ETutorialEventType.UpdateProgress, progress);
    }

    public class SongTutorialStateController : ITutorialController
    {
        private ETutorialState cachedTutorialState;
        private TutorialData tutorialData;
        private float tutorialStartTime;
        private bool isRunning = false;
        private bool hasStarted = false;

        public bool IsCompleted => tutorialData.IsCompleted;

        public readonly Subject<TutorialEvent> TutorialEventSubject = new();

        public void UpdateState(ETutorialState currentState) => cachedTutorialState = currentState;

        public void Init(TutorialData tutorialData)
        {
            this.tutorialData = tutorialData;
            UpdateState(ETutorialState.Intro);
            ShowIntro();
        }

        public void ChangeState(bool isPlaying)
        {
            if (isPlaying && !hasStarted)
            {
                hasStarted = true;
                tutorialStartTime = 0f;
            }
            isRunning = isPlaying;
        }

        public void Tick()
        {
            if (!isRunning) return;

            if (hasStarted && tutorialStartTime <= 0f) tutorialStartTime = Time.time;

            if (cachedTutorialState.EnumEquals(ETutorialState.Step))
            {
                if (Time.time - tutorialStartTime >= tutorialData.GetCurrentStep().TriggerTime)
                {
                    UpdateState(ETutorialState.Step);
                    ShowCurrentStep();
                }
            }
        }

        public void CompleteCurrentStep()
        {
            tutorialData.CompleteCurrentStep();
            UpdateProgressDisplay();
            if (tutorialData.IsCompleted)
            {
                UpdateState(ETutorialState.Complete);
                ShowComplete();
                return;
            }
            ShowCurrentStep();
        }

        public void SkipCurrentStep()
        {
            var currentStep = tutorialData.GetCurrentStep();
            if (currentStep.IsSkippable)
            {
                tutorialData.SkipCurrentStep();
                UpdateProgressDisplay();

                if (tutorialData.IsCompleted)
                {
                    UpdateState(ETutorialState.Complete);
                    ShowComplete();
                    return;
                }
                ShowCurrentStep();
            }
        }

        private void ShowIntro() => TutorialEventSubject.OnNext(TutorialEvent.ShowIntro(tutorialData.TutorialMaster));
        private void ShowCurrentStep() => TutorialEventSubject.OnNext(TutorialEvent.ShowStep((tutorialData.GetCurrentStep(), false)));
        private void ShowComplete() => TutorialEventSubject.OnNext(TutorialEvent.ShowComplete());
        public void Hide() => TutorialEventSubject.OnNext(TutorialEvent.Hide());
        private void UpdateProgressDisplay() => TutorialEventSubject.OnNext(TutorialEvent.UpdateProgress((tutorialData.CompletedCount, tutorialData.TotalCount)));
    }
}