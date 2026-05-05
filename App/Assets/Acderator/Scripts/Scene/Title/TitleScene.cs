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
    [SceneType(ESceneType.Title)]
    public class TitleScene : SceneBase
    {
        [SerializeField] private CommonButton startButton;
        [SerializeField] private TextMeshProUGUI startText;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private FailFastExceptionWatcher failFastExceptionWatcher;

        [Inject] private TitleAuthController titleAuthController;
        [Inject] private NetworkManager networkManager;
        [Inject] private Song.TutorialSceneContextBuilder tutorialSceneContextBuilder;

        protected override void Start()
        {
            startButton.OnTapButtonAsObservable.SubscribeLockAwait(new(true), async (_, ct) =>
            {
                while (!await titleAuthController.ExecuteAsync(ct, failFastExceptionWatcher)) { }
                await ChangeNextScene();
            }).RegisterTo(destroyCancellationToken);
            base.Start();
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
                await sceneManager.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                return;
            }

            if (!tutorialSceneContextBuilder.TryGetFirstTutorialScoreId(out var scoreId))
            {
                await sceneManager.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
                return;
            }

            var request = new ScoreBeginRequest();
            request.PostData.Add("score_id", scoreId);
            var response = await networkManager.RequestAsync(request) as ScoreBeginResponse;
            if ((response?.IsSuccess ?? false) && tutorialSceneContextBuilder.TryBuildFirstTutorial(response.SessionId, out var tutorialContext))
            {
                await sceneManager.ChangeSceneAsync(ESceneType.Song, tutorialContext);
                return;
            }

            await sceneManager.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
        }

        private async UniTask AutoDownloadAllAssets()
        {
            while (!await titleAuthController.ExecuteAsync(destroyCancellationToken, failFastExceptionWatcher)) { }
            PlayerPrefsValues.Set(PlayerPrefsKey.TutorialCompletedNeedsFullDownload, false);
            await ChangeNextScene();
        }
    }
}
