using Cysharp.Threading.Tasks;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Attribute;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using R3;
using UnityEngine;
using Zenject;

namespace Result
{
    [SceneType(ESceneType.Result)]
    public class ResultScene : SceneBase
    {
        [SerializeField] private AtlasImage backgroundImage;
        [SerializeField] private CommonButton retryButton;
        [SerializeField] private CommonButton quitButton;
        [SerializeField] private ResultDetail resultDetail;
        [SerializeField] private GameObject cautionTextRoot;
        [SerializeField] private ResultVisualConfig resultVisualConfig;

        [Inject] private readonly ResultSceneContext sceneContext;
        [Inject] private readonly AddressableAssetManager addressableAssetManager;
        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly SoundManager soundManager;
        [Inject] private readonly NetworkManager networkManager;

        private void Start()
        {
            retryButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, __) => await TapRetryButton()).RegisterTo(destroyCancellationToken);
            quitButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, ___) => await sceneManager.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext())).RegisterTo(destroyCancellationToken);
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            await addressableAssetManager.LoadAssetsAsync(sceneManager.CurrentSceneType, destroyCancellationToken);
            var song = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sceneContext.ResultInfo.Sid);
            var rank = (int)ScoreUtils.ToRank(sceneContext.ResultInfo.CurrentScore);
            resultDetail.Setup(
                song,
                sceneContext.ResultInfo,
                addressableAssetManager.GetSprite(song.Group.ToString()),
                addressableAssetManager.GetSprite(string.Format("icon_result_rank_{0}", rank)));
            backgroundImage.SetSprite(addressableAssetManager.GetSprite(masterDataManager.MemoryDatabase.ResultMasterTable.First().Rid.ToString()));
            backgroundImage.color = ScoreUtils.IsClear(sceneContext.ResultInfo.CurrentScore) ? resultVisualConfig.ClearBackgroundColor : resultVisualConfig.FailedBackgroundColor;
            cautionTextRoot.SetActive(sceneContext.ResultInfo.IsAuto);
            retryButton.gameObject.SetActive(!sceneContext.ResultInfo.IsAuto);

            await sceneManager.FadeInAsync();
            soundManager.PlayBgm(ScoreUtils.IsClear(sceneContext.ResultInfo.CurrentScore) ? EBgmType.GameResult : EBgmType.GameResultFailed);
        });

        private async UniTask TapRetryButton()
        {
            var request = new ScoreBeginRequest { ScoreId = sceneContext.ResultInfo.Sid };
            var response = await networkManager.RequestAsync(request);
            if (!(response?.IsSuccess ?? false)) return;
            await sceneManager.ChangeSceneAsync(ESceneType.Song, Song.SongSceneContext.Create(new(masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sceneContext.ResultInfo.Sid)), sceneContext.ResultInfo.IsAuto, Song.ESongMode.Normal, response.SessionId));
        }
    }
}