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
using uPalette.Generated;
using uPalette.Runtime.Core;
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

        [Inject] private ResultSceneContext sceneContext;
        [Inject] private AssetBundleManager assetBundleManager;
        [Inject] private MasterDataManager masterDataManager;
        [Inject] private SoundManager soundManager;
        [Inject] private NetworkManager networkManager;

        private void Start()
        {
            retryButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, __) => await TapRetryButton()).RegisterTo(destroyCancellationToken);
            quitButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, ___) => await sceneManager.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext())).RegisterTo(destroyCancellationToken);
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            sceneContext.ResultSceneBundlePathList.ForEach(assetBundleManager.AddLoadAssets);
            await assetBundleManager.LoadAssetsAsync(sceneManager.CurrentSceneType, destroyCancellationToken);
            resultDetail.Setup(masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sceneContext.ResultInfo.Sid), sceneContext.ResultInfo);
            backgroundImage.SetAtlasFormat("{0}", masterDataManager.MemoryDatabase.ResultMasterTable.First().Rid, "gameresult/bg");
            backgroundImage.color = PaletteStore.Instance.ColorPalette.GetActiveValue((ScoreUtils.IsClear(sceneContext.ResultInfo.CurrentScore) ? ColorEntry.White : ColorEntry.LightWhite).ToEntryId()).Value;
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