using Cysharp.Threading.Tasks;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using System;
using System.Threading;

namespace Title
{
    public class TitleAuthController
    {
        public async UniTask<bool> ExecuteAsync(CancellationToken token, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var userid = LocalDataManager.Instance.LocalUser.UserId;
            RequestBase request;
            if (string.IsNullOrEmpty(userid))
            {
                request = new RegisterRequest();
                request.PostData.Add("uuid", Guid.NewGuid().ToString());
            }
            else
            {
                request = new LoginRequest();
                var password = LocalDataManager.Instance.LocalUser.PassWard;
                request.PostData.Add("userid", userid);
                request.PostData.Add("password", password);
            }
            var authResponse = await NetworkManager.Instance.RequestAsync(request).AddWatcherTo(failFastExceptionWatcher);
            if (authResponse?.Status != 200) return await PopupUtils.TryOpenNetworkErrorPopup(authResponse);
            if (authResponse is RegisterResponse rr)
            {
                LocalDataManager.Instance.LocalUser.UserId = rr.UserId.ToString();
                LocalDataManager.Instance.LocalUser.PassWard = rr.PassWord;
                LocalDataManager.Instance.System.Token = rr.Token;
            }
            else
            {
                LocalDataManager.Instance.System.Token = (authResponse as LoginResponse).Token;
            }
            await LoadAssets(token).AddWatcherTo(failFastExceptionWatcher);
            var userDataResponse = await NetworkManager.Instance.RequestAsync(new UserDataRequest()).AddWatcherTo(failFastExceptionWatcher) as UserDataResponse;
            if (userDataResponse?.Status != 200) return await PopupUtils.TryOpenNetworkErrorPopup(userDataResponse);
            await MasterDataManager.Instance.LoadMasterAsync();
            ScoreManager.Instance.SetScoreData(userDataResponse.Scores);
            await SoundManager.Instance.InitializeAsync();
            await SceneManager.Instance.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
            return true;
        }

        private async UniTask LoadAssets(CancellationToken token)
        {
            AssetBundleManager.Instance.NotExistAssetBundleName.ForEach(AssetBundleManager.Instance.AddLoadAssets);
            await AssetBundleManager.Instance.LoadAssetsAsync(token);
        }
    }
}