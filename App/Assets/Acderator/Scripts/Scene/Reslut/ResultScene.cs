using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
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
    public class ResultScene : SceneBase
    {
        [SerializeField] private AtlasImage backgroundImage;
        [SerializeField] private CommonButton retryButton;
        [SerializeField] private CommonButton quitButton;
        [SerializeField] private ResultDetail resultDetail;
        [SerializeField] private GameObject cautionTextRoot;

        [Inject] private ResultSceneContext sceneContext;

        protected override void Start()
        {
            retryButton.OnTapButtonAsObservable.SubscribeLockAwait(new(true), async (_, __) => await SceneManager.Instance.ChangeSceneAsync(ESceneType.Song, Song.SongSceneContext.Create(new(sceneContext.ResultInfo.Sid), sceneContext.ResultInfo.IsAuto, Song.ESongMode.Normal))).RegisterTo(destroyCancellationToken);
            quitButton.OnTapButtonAsObservable.SubscribeLockAwait(new(true), async (_, ___) => await SceneManager.Instance.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext())).RegisterTo(destroyCancellationToken);
            base.Start();
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            sceneContext.ResultSceneBundlePathList.ForEach(AssetBundleManager.Instance.AddLoadAssets);
            await AssetBundleManager.Instance.LoadAssetsAsync(destroyCancellationToken);
            resultDetail.Setup(sceneContext.ResultInfo);
            backgroundImage.SetAtlasFormat("{0}", MasterDataManager.Instance.MemoryDatabase.ResultMasterTable.First().Rid, "gameresult/bg");
            backgroundImage.color = PaletteStore.Instance.ColorPalette.GetActiveValue((ScoreUtils.IsClear(sceneContext.ResultInfo.CurrentScore) ? ColorEntry.White : ColorEntry.LightWhite).ToEntryId()).Value;
            cautionTextRoot.SetActive(sceneContext.ResultInfo.IsAuto);
            retryButton.gameObject.SetActive(!sceneContext.ResultInfo.IsAuto);

            await SceneManager.Instance.FadeInAsync();
            SoundManager.Instance.PlayBgm(ScoreUtils.IsClear(sceneContext.ResultInfo.CurrentScore) ? EBgmType.GameResult : EBgmType.GameResultFailed);
        });
    }
}