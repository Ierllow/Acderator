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
        [Inject] private readonly AddressableAssetManager addressableAssetManager;
        [Inject] private readonly PopupManager popupManager;

        internal NetworkManager NetworkManager => networkManager;

        public async UniTask<bool> Execute(CancellationToken token, FailFastExceptionWatcher failFastExceptionWatcher)
        {
            var request = CreateAuthRequest();
            var authResponse = await RequestAuth(request, failFastExceptionWatcher);
            if (!(authResponse?.IsSuccess ?? false)) return await OpenNetworkErrorPopup(authResponse);

            if (authResponse is RegisterResponse registerResponse)
            {
                PlayerPrefsValues.Set(PlayerPrefsKey.UserId, registerResponse.UserId);
                PlayerPrefsValues.Set(PlayerPrefsKey.Password, registerResponse.Password);
                networkManager.Token = registerResponse.Token;
            }
            else
            {
                networkManager.Token = (authResponse as LoginResponse)?.Token;
            }

            await addressableAssetManager.LoadAssetsAsync(SceneType.Title, token).AddWatcherTo(failFastExceptionWatcher);
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
            if (response?.NetworkError != NetworkError.PreconditionFailed) return response;

            await masterDataManager.LoadMasterAsync(response.Master).AddWatcherTo(failFastExceptionWatcher);
            return await networkManager.RequestAsync(request).AddWatcherTo(failFastExceptionWatcher);
        }

        private async UniTask<bool> OpenNetworkErrorPopup(ResponseBase response)
        {
            var completionSource = AutoResetUniTaskCompletionSource<CommonPopupTapKind>.Create();
            var context = PopupContextFactory.CreateNetworkErrorPopupContext(completionSource, response?.ErrorMessage ?? "通信に失敗しました。", response?.ErrorCode ?? 0);
            popupManager.OpenPopup(context);
            return await completionSource.Task == CommonPopupTapKind.Negative;
        }
    }
}