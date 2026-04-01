using Cysharp.Threading.Tasks;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Title
{
    public class TitleAuthController
    {
        public async UniTask<bool> ExecuteAsync(CancellationToken token, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            static async UniTask loadMasterFunc(Dictionary<string, object> masterDict) => await MasterDataManager.Instance.LoadMasterAsync(masterDict);

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
                await loadMasterFunc(rr.Master).AddWatcherTo(failFastExceptionWatcher);
            }
            else
            {
                LocalDataManager.Instance.System.Token = (authResponse as LoginResponse).Token;
                await loadMasterFunc((authResponse as LoginResponse).Master).AddWatcherTo(failFastExceptionWatcher);
            }
            await LoadAssets(token).AddWatcherTo(failFastExceptionWatcher);
            var userDataResponse = await NetworkManager.Instance.RequestAsync(new UserDataRequest()).AddWatcherTo(failFastExceptionWatcher) as UserDataResponse;
            if (userDataResponse?.Status != 200) return await PopupUtils.TryOpenNetworkErrorPopup(userDataResponse);
            ScoreManager.Instance.SetScoreData(userDataResponse.Scores);
            await SoundManager.Instance.InitializeAsync();
            return true;
        }

        private async UniTask LoadAssets(CancellationToken token)
        {
            AssetBundleManager.Instance.NotExistAssetBundleName.ForEach(AssetBundleManager.Instance.AddLoadAssets);
            await AssetBundleManager.Instance.LoadAssetsAsync(token);
        }
    }
}