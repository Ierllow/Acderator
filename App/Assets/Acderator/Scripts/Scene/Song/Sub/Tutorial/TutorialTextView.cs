using Intense.Master;
using TMPro;
using UnityEngine;
using Zenject;

namespace Song
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TutorialTextView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Inject] private TutorialMessageResolver tutorialMessageResolver;

        public void ShowIntro(TutorialMaster tutorial) => descriptionText.SetText(tutorial.Description);

        public void ShowStep(TutorialStepMaster tutorialStep) => descriptionText.SetText(tutorialMessageResolver.GetStepHint(tutorialStep));

        public void ShowComplete() => descriptionText.SetText("お疲れさまでした。基本操作をマスターしました！");
    }
}