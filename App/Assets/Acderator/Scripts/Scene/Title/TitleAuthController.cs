using Cysharp.Threading.Tasks;
using Element.UI;
using Intense;
using Intense.Api;
using Intense.Data;
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
                var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
                PopupManager.Instance.OpenPopup(PopupContextFactory.CreateNetworkErrorPopupContext(completionSource, response.Error));
                var result = await completionSource.Task;
                return result.EnumEquals(ECommonPopupTapKind.Negative);
            }

            ScoreManager.Instance.SetScoreData(response?.Result.InfoResultPayload?.UserData);

            if (!ResourceManager.Instance.IsInit)
            {
                DownloadSizeConfManager.Instance.SetProgressInActive();
                DownloadSizeConfManager.Instance.SetDownloadSizePopupNotRun();
                await ResourceManager.Instance.OnLoadGameAssetAsync().AddWatcherTo(failFastExceptionWatcher);
            }
            await SceneManager.Instance.ChangeSceneAsync(ESceneType.SongSelect, new SongSelect.SongSelectSceneContext());
            return true;
        }
    }
}