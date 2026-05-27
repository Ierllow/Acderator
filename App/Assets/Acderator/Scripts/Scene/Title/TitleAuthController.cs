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
        [Inject] private readonly NetworkManager networkManager;
        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly ScoreManager scoreManager;
        [Inject] private readonly AssetBundleManager assetBundleManager;
        [Inject] private readonly PopupManager popupManager;
        [Inject] private readonly IApiSession apiSession;

        public async UniTask<bool> Execute(CancellationToken token, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var request = CreateAuthRequest();
            var authResponse = await RequestAuth(request, failFastExceptionWatcher);
            if (!(authResponse?.IsSuccess ?? false)) return await OpenNetworkErrorPopup(authResponse);

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
            var userDataResponse = await networkManager.RequestAsync(new UserDataRequest()).AddWatcherTo(failFastExceptionWatcher);
            if (!(userDataResponse?.IsSuccess ?? false)) return await OpenNetworkErrorPopup(userDataResponse);
            scoreManager.SetScoreData(userDataResponse.ScorePairs);
            return true;
        }

        private RequestBase CreateAuthRequest()
        {
            var userid = PlayerPrefsValues.UserId;
            return string.IsNullOrEmpty(userid) ? new RegisterRequest { Uuid = Guid.NewGuid().ToString() } : new LoginRequest { UserId = userid, Password = PlayerPrefsValues.Password };
        }

        private async UniTask<ResponseBase> RequestAuth(RequestBase request, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var response = await networkManager.RequestAsync(request).AddWatcherTo(failFastExceptionWatcher);
            if (response?.NetworkError != NetworkError.PreconditionFailed || response.Master is not { } master) return response;

            await masterDataManager.LoadMasterAsync(master).AddWatcherTo(failFastExceptionWatcher);
            return await networkManager.RequestAsync(request).AddWatcherTo(failFastExceptionWatcher);
        }

        private async UniTask<bool> OpenNetworkErrorPopup(ResponseBase response)
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