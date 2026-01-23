using Cysharp.Threading.Tasks;
using Element.UI;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using PlayFab;
using System;
using System.Threading;

namespace Title
{
    public class TitleAuthController
    {
        public async UniTask<bool> ExecuteAsync(CancellationToken token, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var response = await NetworkManager.Instance.LogInAnonymouslyAsync().AddWatcherTo(failFastExceptionWatcher);
            if (response?.Error != default && !response.Error.Error.EnumEquals(PlayFabErrorCode.Success))
            {
                var result = await PopupUtils.OpenNetworkErrorPopup(response.Error);
                return result.EnumEquals(ECommonPopupTapKind.Negative);
            }

            await LoadAssets(token).AddWatcherTo(failFastExceptionWatcher);
            ScoreManager.Instance.SetScoreData(response?.Result.InfoResultPayload?.UserData);
            await SoundManager.Instance.InitializeAsync();
            await SceneManager.Instance.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
            return true;
        }

        private async UniTask LoadAssets(CancellationToken token)
        {
            AssetBundleManager.Instance.NotExistAssetBundleName.ForEach(AssetBundleManager.Instance.AddLoadAssets);
            AssetBundleManager.Instance.AddLoadAssets("master/master");
            await AssetBundleManager.Instance.LoadAssetsAsync(token);
            await MasterDataManager.Instance.LoadMasterAsync();
        }
    }
}