using Cysharp.Threading.Tasks;
using DG.Tweening;
using Intense;
using Intense.Api;
using Intense.Attribute;
using Intense.UI;
using R3;
using TMPro;
using UnityEngine;
using Zenject;

namespace Title
{
    [SceneType(SceneType.Title)]
    public class TitleScene : SceneBase
    {
        [SerializeField] private CommonButton startButton;
        [SerializeField] private TextMeshProUGUI startText;
        [SerializeField] private TextMeshProUGUI versionText;

        [Inject] private readonly TitleAuthController titleAuthController;
        [Inject] private readonly Song.TutorialSceneContextBuilder tutorialSceneContextBuilder;
        [Inject] private readonly FailFastExceptionWatcher failFastExceptionWatcher;

        protected override void Awake()
        {
            failFastExceptionWatcher.Init(destroyCancellationToken);
            base.Awake();
        }

        private void Start()
        {
            startButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, ct) =>
            {
                while (!await titleAuthController.Execute(ct, failFastExceptionWatcher)) { }
                await ChangeNextScene();
            }).RegisterTo(destroyCancellationToken);
        }

        public override void OnCreateScene()
        {
            versionText.SetText(string.Format("Version {0}", Application.version));
            startText.DOFade(0, 1).SetEase(Ease.Flash, 1).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
            sceneManager.FadeInAsync().Forget();
            if (PlayerPrefsValues.IsTutorialCompletedNeedsFullDownload) AutoDownloadAllAssets().Forget();
        }

        private async UniTask ChangeNextScene()
        {
            if (PlayerPrefsValues.IsTutorialCompleted)
            {
                await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                return;
            }

            if (!tutorialSceneContextBuilder.TryGetFirstTutorialScoreId(out var scoreId))
            {
                await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                return;
            }

            var request = new ScoreBeginRequest { ScoreId = scoreId };
            var response = await titleAuthController.NetworkManager.RequestAsync(request);
            if ((response?.IsSuccess ?? false) && tutorialSceneContextBuilder.TryBuildFirstTutorial(response.SessionId, out var tutorialContext))
            {
                await sceneManager.ChangeSceneAsync(SceneType.Song, tutorialContext);
                return;
            }

            await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext());
        }

        private async UniTask AutoDownloadAllAssets()
        {
            while (!await titleAuthController.Execute(destroyCancellationToken, failFastExceptionWatcher)) { }
            PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompletedNeedsFullDownload, false);
            await ChangeNextScene();
        }
    }
}