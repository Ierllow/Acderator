using DG.Tweening;
using Intense.Master;
using R3;
using System;
using UnityEngine;

namespace Song
{
    public class SongTutorialLayerController : MonoBehaviour, IController
    {
        private enum TutorialLayerState { Hidden, Intro, WaitingForTap, Fading }

        private const float FadeOutDuration = 0.35f;

        [SerializeField] private TutorialTextView tutorialTextView;
        [SerializeField] private GameObject tutorialOverlay;
        [SerializeField] private CanvasGroup bodyCanvasGroup;

        private TutorialStepMaster currentStep;
        private TutorialLayerState currentState = TutorialLayerState.Hidden;
        private Tween fadeTween;

        public readonly Subject<Unit> TutorialCompletedSubject = new();
        public readonly Subject<Unit> TutorialIntroCompletedSubject = new();
        public readonly Subject<Unit> TutorialStepCompletedSubject = new();

        private bool CanAdvanceByTap(bool canAdvance) => canAdvance && currentState is TutorialLayerState.Intro or TutorialLayerState.WaitingForTap;

        public void AdvanceByTapIfPossible(bool canAdvance)
        {
            if (CanAdvanceByTap(canAdvance) && IsAdvanceTapStarted()) CompleteCurrentStep();
        }

        public void UpdateTutorial(TutorialEvent tutorialEvent)
        {
            switch (tutorialEvent.Type)
            {
                case ETutorialEventType.ShowIntro:
                    ShowTutorialIntro((TutorialMaster)tutorialEvent.Data);
                    break;
                case ETutorialEventType.ShowStep:
                    ShowTutorialStep((TutorialStepMaster)tutorialEvent.Data);
                    break;
                case ETutorialEventType.ShowComplete:
                    ShowTutorialComplete();
                    break;
                default:
                    break;
            }
        }

        private void ShowTutorialIntro(TutorialMaster tutorial)
        {
            currentStep = default;
            currentState = TutorialLayerState.Intro;
            tutorialOverlay.SetActive(true);
            ShowBody();
            tutorialTextView.ShowIntro(tutorial);
        }

        private void ShowTutorialStep(TutorialStepMaster tutorialStep)
        {
            currentStep = tutorialStep;
            currentState = TutorialLayerState.WaitingForTap;
            tutorialOverlay.SetActive(true);
            ShowBody();
            tutorialTextView.ShowStep(tutorialStep);
        }

        private void ShowTutorialComplete()
        {
            currentStep = default;
            currentState = TutorialLayerState.WaitingForTap;
            tutorialOverlay.SetActive(true);
            ShowBody();
            tutorialTextView.ShowComplete();
        }

        public void Hide()
        {
            currentState = TutorialLayerState.Hidden;
            fadeTween?.Kill();
            tutorialOverlay.SetActive(false);
            bodyCanvasGroup.alpha = 0f;
            bodyCanvasGroup.blocksRaycasts = false;
        }

        public void CompleteCurrentStep()
        {
            switch (currentState)
            {
                case TutorialLayerState.Intro:
                    FadeOut(() => TutorialIntroCompletedSubject.OnNext(Unit.Default));
                    break;
                case TutorialLayerState.WaitingForTap when currentStep == default:
                    CompleteTutorial();
                    break;
                case TutorialLayerState.WaitingForTap:
                    FadeOut(() => TutorialStepCompletedSubject.OnNext(Unit.Default));
                    break;
                default:
                    break;
            }
        }

        public void CompleteTutorial() => FadeOut(() => TutorialCompletedSubject.OnNext(Unit.Default));

        private bool IsAdvanceTapStarted() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);

        private void FadeOut(Action onComplete)
        {
            currentState = TutorialLayerState.Fading;
            fadeTween?.Kill();
            fadeTween = bodyCanvasGroup.DOFade(0f, FadeOutDuration).SetLink(gameObject).OnComplete(() =>
            {
                Hide();
                onComplete?.Invoke();
            });
        }

        private void ShowBody()
        {
            fadeTween?.Kill();
            bodyCanvasGroup.alpha = 1f;
            bodyCanvasGroup.blocksRaycasts = true;
        }
    }
}