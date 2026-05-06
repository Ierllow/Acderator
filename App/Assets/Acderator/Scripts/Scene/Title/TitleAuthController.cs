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
        [Inject] private IApiSession apiSession;

        public async UniTask<bool> ExecuteAsync(CancellationToken token, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var request = CreateAuthRequest();
            var authResponse = await RequestAuthAsync(request, failFastExceptionWatcher);
            if (!(authResponse?.IsSuccess ?? false)) return await OpenNetworkErrorPopupAndIsCloseAsync(authResponse);

            if (authResponse is RegisterResponse registerResponse)
            {
                PlayerPrefsValues.Set(PlayerPrefsKey.UserId, registerResponse.UserId);
                PlayerPrefsValues.Set(PlayerPrefsKey.Password, registerResponse.Password);
                apiSession.Token = registerResponse.Token;
            }
            else
            {
                apiSession.Token = (authResponse as LoginResponse)?.Token;
            }

            await LoadAssets(token).AddWatcherTo(failFastExceptionWatcher);
            var userDataResponse = await networkManager.RequestAsync(new UserDataRequest()).AddWatcherTo(failFastExceptionWatcher) as UserDataResponse;
            if (!(userDataResponse?.IsSuccess ?? false)) return await OpenNetworkErrorPopupAndIsCloseAsync(userDataResponse);
            scoreManager.SetScoreData(userDataResponse.Scores);
            return true;
        }

        private static RequestBase CreateAuthRequest()
        {
            var userid = PlayerPrefsValues.UserId;
            if (string.IsNullOrEmpty(userid))
            {
                var request = new RegisterRequest();
                request.PostData.Add("uuid", Guid.NewGuid().ToString());
                return request;
            }
            else
            {
                var request = new LoginRequest();
                request.PostData.Add("userid", userid);
                request.PostData.Add("password", PlayerPrefsValues.Password);
                return request;
            }
        }

        private async UniTask<ResponseBase> RequestAuthAsync(RequestBase request, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var response = await networkManager.RequestAsync(request).AddWatcherTo(failFastExceptionWatcher);
            if (response?.NetworkError != NetworkError.PreconditionFailed || response.Master is not { } master) return response;

            await masterDataManager.LoadMasterAsync(master).AddWatcherTo(failFastExceptionWatcher);
            return await networkManager.RequestAsync(request).AddWatcherTo(failFastExceptionWatcher);
        }

        private async UniTask<bool> OpenNetworkErrorPopupAndIsCloseAsync(ResponseBase response)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            var context = PopupContextFactory.CreateNetworkErrorPopupContext(completionSource, response?.ErrorMessage ?? "通信に失敗しました。", response?.ErrorCode ?? 0);
            popupManager.OpenPopup(context);
            return await completionSource.Task == ECommonPopupTapKind.Negative;
        }

        private async UniTask LoadAssets(CancellationToken token)
        {
            assetBundleManager.NotExistAssetBundleName.ForEach(assetBundleManager.AddLoadAssets);
            await assetBundleManager.LoadAssetsAsync(ESceneType.Title, token);
        }
    }
}