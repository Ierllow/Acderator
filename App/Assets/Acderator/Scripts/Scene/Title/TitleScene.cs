using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Intense;
using Intense.UI;
using R3;
using TMPro;
using UnityEngine;
using Zenject;

namespace Title
{
    public class TitleScene : SceneBase
    {
        [SerializeField] private CommonButton startButton;
        [SerializeField] private TextMeshProUGUI startText;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private FailFastExceptionWatcher failFastExceptionWatcher;

        [Inject] private TitleAuthController titleAuthController;

        protected override void Start()
        {
            startButton.OnTapButtonAsObservable.SubscribeLockAwait(new(true), async (_, ct) =>
            {
                while (!await titleAuthController.ExecuteAsync(ct, failFastExceptionWatcher)) { }
            }).RegisterTo(destroyCancellationToken);
            base.Start();
        }

        public override void OnCreateScene()
        {
            versionText.SetTextFormat("Version {0}", Application.version);
            startText.DOFade(0, 1).SetEase(Ease.Flash, 1).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
            SceneManager.Instance.FadeInAsync().Forget();
        }
    }
}