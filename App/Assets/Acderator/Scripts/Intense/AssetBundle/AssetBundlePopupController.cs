using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.UI;
using UnityEngine.Networking;
using Zenject;

namespace Intense.Asset
{
    internal class AssetBundlePopupController
    {
        [Inject] private readonly PopupManager popupManager;

        public async UniTask<bool> TryRetryAssetErrorAsync(UnityWebRequest.Result result)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            var kind = result == UnityWebRequest.Result.ProtocolError
                ? EAssetBundleErrorKind.ProtocolError
                : EAssetBundleErrorKind.ConnectionError;
            var popupContext = PopupContextFactory.CreateAssetErrorPopupContext(completionSource, kind);
            popupManager.OpenPopup(popupContext);
            return await completionSource.Task == ECommonPopupTapKind.Positive;
        }

        public async UniTask<bool> TryDownloadConfirmedAsync(long fileSize)
        {
            if (fileSize <= 0) return true;

            var context = PopupContextFactory.CreateDownloadSizeConfirmPopupContext(fileSize.GetFileSize(), fileSize.GetFileSizeType().GetText());
            popupManager.OpenPopup(context);

            var popup = popupManager.CurrentOpenPopup as DownloadSizeConfPopup;
            await UniTask.WaitUntil(() => popup.IsClose);
            return popup.IsConfirm;
        }
    }
}