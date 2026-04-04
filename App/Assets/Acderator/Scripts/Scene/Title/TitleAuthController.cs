using Cysharp.Threading.Tasks;
using Element.UI;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using System;
using System.Threading;
using Zenject;

namespace Title
{
    public class TitleAuthController
    {
        [Inject] private NetworkManager networkManager;
        [Inject] private MasterDataManager masterDataManager;
        [Inject] private ScoreManager scoreManager;
        [Inject] private AssetBundleManager assetBundleManager;
        [Inject] private PopupManager popupManager;

        public async UniTask<bool> ExecuteAsync(CancellationToken token, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var userid = PlayerPrefsValues.UI;
            RequestBase request;
            if (string.IsNullOrEmpty(userid))
            {
                request = new RegisterRequest();
                request.PostData.Add("uuid", Guid.NewGuid().ToString());
            }
            else
            {
                request = new LoginRequest();
                var password = PlayerPrefsValues.PW;
                request.PostData.Add("userid", userid);
                request.PostData.Add("password", password);
            }
            var authResponse = await networkManager.RequestAsync(request).AddWatcherTo(failFastExceptionWatcher);
            if (authResponse?.Status != 200) return await OpenNetworkErrorPopupAndIsCloseAsync(authResponse);
            if (authResponse is RegisterResponse rr)
            {
                PlayerPrefsValues.Set(EKey.UserId, rr.UserId);
                PlayerPrefsValues.Set(EKey.PassWard, rr.PassWord);
                PlayerPrefsValues.TK = rr.Token;
                await masterDataManager.LoadMasterAsync(rr.Master).AddWatcherTo(failFastExceptionWatcher);
            }
            else
            {
                PlayerPrefsValues.TK = (authResponse as LoginResponse).Token;
                await masterDataManager.LoadMasterAsync((authResponse as LoginResponse).Master).AddWatcherTo(failFastExceptionWatcher);
            }
            await LoadAssets(token).AddWatcherTo(failFastExceptionWatcher);
            var userDataResponse = await networkManager.RequestAsync(new UserDataRequest()).AddWatcherTo(failFastExceptionWatcher) as UserDataResponse;
            if (userDataResponse?.Status != 200) return await OpenNetworkErrorPopupAndIsCloseAsync(userDataResponse);
            scoreManager.SetScoreData(userDataResponse.Scores);
            return true;
        }

        private async UniTask<bool> OpenNetworkErrorPopupAndIsCloseAsync(ResponseBase response)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            var context = PopupContextFactory.CreateNetworkErrorPopupContext(completionSource, response.ErrorMessage, response.Status);
            popupManager.OpenPopup(context);
            return (await completionSource.Task).EnumEquals(ECommonPopupTapKind.Negative);
        }

        private async UniTask LoadAssets(CancellationToken token)
        {
            assetBundleManager.NotExistAssetBundleName.ForEach(assetBundleManager.AddLoadAssets);
            await assetBundleManager.LoadAssetsAsync(ESceneType.Title, token);
        }
    }
}