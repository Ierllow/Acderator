using Cysharp.Text;
using Intense.Master;
using Intense.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Song
{
    public class SongTutorialLayerController : MonoBehaviour, IController
    {
        [SerializeField] private GameObject tutorialOverlay;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private GameObject arrowIndicator;
        [SerializeField] private GameObject highlightArea;
        [SerializeField] private CommonButton skipButton;
        [SerializeField] private CommonButton nextButton;
        [SerializeField] private GameObject progressIndicator;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private (TutorialStepMaster, bool) currentStepTutoriaTuple = default;

        public readonly ReactiveProperty<Unit> TutorialCompletedReactiveProperty = new(default);

        private void Awake()
        {
            skipButton.OnTapButtonAsObservable.SubscribeLock(new(), _ => SkipCurrentStep()).RegisterTo(destroyCancellationToken);
            nextButton.OnTapButtonAsObservable.SubscribeLock(new(), _ => CompleteCurrentStep()).RegisterTo(destroyCancellationToken);
        }

        public void UpdateTutorial(TutorialEvent tutorialEvent)
        {
            switch (tutorialEvent.Type)
            {
                case ETutorialEventType.ShowIntro:
                    ShowTutorialIntro((TutorialMaster)tutorialEvent.Data);
                    break;
                case ETutorialEventType.ShowStep:
                    ShowTutorialStep(((TutorialStepMaster, bool))tutorialEvent.Data);
                    break;
                case ETutorialEventType.ShowComplete:
                    ShowTutorialComplete();
                    break;
                case ETutorialEventType.Hide:
                    Hide();
                    break;
                case ETutorialEventType.UpdateProgress:
                    var (completed, total) = ((int, int))tutorialEvent.Data;
                    UpdateProgressDisplay(completed, total);
                    break;
                default:
                    break;
            }
        }

        private void ShowTutorialIntro(TutorialMaster tutorial)
        {
            tutorialOverlay.SetActive(true);

            titleText.SetText(tutorial.Title);
            descriptionText.SetText(tutorial.Description);

            skipButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
        }

        private void ShowTutorialStep((TutorialStepMaster, bool) tutorialStepTuple)
        {
            currentStepTutoriaTuple = tutorialStepTuple;

            tutorialOverlay.SetActive(true);

            hintText.SetText(tutorialStepTuple.Item1.Description);
            hintText.transform.localPosition = new Vector3(tutorialStepTuple.Item1.HintPositionX, tutorialStepTuple.Item1.HintPositionY, hintText.transform.localPosition.z);

            skipButton.gameObject.SetActive(tutorialStepTuple.Item1.IsSkippable);
            nextButton.gameObject.SetActive(tutorialStepTuple.Item1.IsSkippable);
        }

        private void ShowTutorialComplete()
        {
            titleText.SetText("チュートリアル完了！");
            descriptionText.SetText("お疲れさまでした。基本操作をマスターしました！");

            skipButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
        }

        public void Hide()
        {
            tutorialOverlay.SetActive(false);
            arrowIndicator.SetActive(false);
            highlightArea.SetActive(false);
        }

        public void CompleteCurrentStep()
        {
            Hide();
            if (currentStepTutoriaTuple.Item2)
            {
                TutorialCompletedReactiveProperty.Value = new();
            }
        }

        public void SkipCurrentStep()
        {
            if (currentStepTutoriaTuple.Item1.IsSkippable)
            {
                currentStepTutoriaTuple.Item2 = true;
                Hide();
                TutorialCompletedReactiveProperty.Value = new();
            }
        }

        public void CompleteTutorial()
        {
            Hide();
            TutorialCompletedReactiveProperty.Value = new();
        }

        private void UpdateProgressDisplay(int completedSteps, int totalSteps)
        {
            progressText.SetTextFormat("{0}/{1}", completedSteps, totalSteps);
            // progressIndicator.fillAmount = (float)completedSteps / totalSteps;
        }

        public void ShowArrow(Vector2 startPos, Vector2 endPos)
        {
            arrowIndicator.SetActive(true);

            var direction = (endPos - startPos).normalized;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            arrowIndicator.transform.position = startPos;
            arrowIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void HighlightArea(RectTransform area)
        {
            highlightArea.SetActive(true);
            highlightArea.transform.position = area.position;
            //highlightArea.transform.sizeDelta = area.sizeDelta;
        }

        public void StopTutorial()
        {
            Hide();
        }
    }
}