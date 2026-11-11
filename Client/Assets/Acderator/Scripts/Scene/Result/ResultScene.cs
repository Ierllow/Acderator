using Acderator.Shared;
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
using UnityEngine.U2D;
using Zenject;

namespace Result
{
    [SceneType(SceneType.Result)]
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

        private static readonly AddressableAssetAddress BackgroundAtlasAddress = new("result/bg");
        private static readonly AddressableAssetAddress JacketAtlasAddress = new("song/jacket");

        private SpriteAtlas backgroundAtlas;
        private SpriteAtlas jacketAtlas;

        private void Start()
        {
            retryButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, __) => await TapRetryButton()).RegisterTo(destroyCancellationToken);
            quitButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, ___) => await sceneManager.ChangeSceneAsync(SceneType.SongSelect, new SongSelect.SongSelectSceneContext())).RegisterTo(destroyCancellationToken);
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            addressableAssetManager.AddLoad(BackgroundAtlasAddress);
            addressableAssetManager.AddLoad(JacketAtlasAddress);
            await addressableAssetManager.LoadAssetsAsync(sceneManager.CurrentSceneType, destroyCancellationToken);
            backgroundAtlas = await addressableAssetManager.LoadAssetAsync<SpriteAtlas>(BackgroundAtlasAddress);
            jacketAtlas = await addressableAssetManager.LoadAssetAsync<SpriteAtlas>(JacketAtlasAddress);
            var song = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sceneContext.ResultInfo.Sid);
            var rank = (int)ScoreUtils.ToRank(sceneContext.ResultInfo.CurrentScore);
            resultDetail.Setup(
                song,
                sceneContext.ResultInfo,
                GetJacketSprite(song.Group),
                addressableAssetManager.GetSprite(string.Format("icon_result_rank_{0}", rank)));
            backgroundImage.SetSprite(GetBackgroundSprite(masterDataManager.MemoryDatabase.ResultMasterTable.First().Rid));
            backgroundImage.color = ScoreUtils.IsClear(sceneContext.ResultInfo.CurrentScore) ? resultVisualConfig.ClearBackgroundColor : resultVisualConfig.FailedBackgroundColor;
            cautionTextRoot.SetActive(sceneContext.ResultInfo.IsAuto);
            retryButton.gameObject.SetActive(!sceneContext.ResultInfo.IsAuto);

            await sceneManager.FadeInAsync();
            soundManager.PlayBgm(ScoreUtils.IsClear(sceneContext.ResultInfo.CurrentScore) ? BgmType.GameResult : BgmType.GameResultFailed);
        });

        private Sprite GetBackgroundSprite(int rid) => backgroundAtlas != null ? backgroundAtlas.GetSprite(rid.ToString()) : addressableAssetManager.GetSprite(rid.ToString());

        private Sprite GetJacketSprite(int group) => jacketAtlas != null ? jacketAtlas.GetSprite(group.ToString()) : addressableAssetManager.GetSprite(group.ToString());

        private async UniTask TapRetryButton()
        {
            var request = new ScoreBeginRequest { ScoreId = sceneContext.ResultInfo.Sid };
            var response = await networkManager.RequestAsync(request);
            if (response?.IsSuccess != true) return;
            await sceneManager.ChangeSceneAsync(SceneType.Song, Song.SongSceneContext.Create(new(masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sceneContext.ResultInfo.Sid)), sceneContext.ResultInfo.IsAuto, Song.SongMode.Normal, response.SessionId));
        }
    }
}