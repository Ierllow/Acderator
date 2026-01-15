using Cysharp.Threading.Tasks;
using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;

namespace Intense
{
    internal class ResourceManager : SingletonMonoBehaviour<ResourceManager>
    {
        public bool IsInit { get; private set; } = false;

        public async UniTask OnLoadGameAssetAsync()
        {
            if (IsInit) return;

            AssetBundleManager.Instance.NotExistAssetBundleName.ForEach(AssetBundleManager.Instance.AddLoadAssets);
            AssetBundleManager.Instance.AddLoadAssets("master/master");

            await AssetBundleManager.Instance.LoadAssetsAsync(destroyCancellationToken);
            await MasterDataManager.Instance.LoadMasterAsync();
            await SoundManager.Instance.InitializeAsync();

            IsInit = true;
            await UniTask.WaitUntil(() => IsInit);
        }

#if UNITY_EDITOR
        public async UniTask ReadyToFirstScene()
        {
            var response = await NetworkManager.Instance.LogInAnonymouslyAsync();
            if (response?.Error != null) return;
            ScoreManager.Instance.SetScoreData(response?.Result.InfoResultPayload?.UserData);
            if (!IsInit) await OnLoadGameAssetAsync();
        }
#endif
    }
}