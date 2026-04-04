using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.Api;
using Intense.Asset;
using System;
using Zenject;

namespace Intense.UI
{
    public static class PopupUtils
    {
        [Inject] private static PopupManager popupManager;

        public static async UniTask OpenErrorPopup()
        {
            var completionSource = AutoResetUniTaskCompletionSource.Create();
            popupManager.OpenPopup(PopupContextFactory.CreateErrorPopupContext(completionSource));
            await completionSource.Task;
        }

        public static async UniTask<ECommonPopupTapKind> OpenNetworkErrorPopup(string error, int errorCode)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            popupManager.OpenPopup(PopupContextFactory.CreateNetworkErrorPopupContext(completionSource, error, errorCode));
            return await completionSource.Task;
        }

        public static async UniTask<ECommonPopupTapKind> OpenAssetErrorPopup(EAssetBundleErrorKind kind, bool titleBake = false)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            popupManager.OpenPopup(PopupContextFactory.CreateAssetErrorPopupContext(completionSource, kind, titleBake));
            return await completionSource.Task;
        }

        public static async UniTask<bool> OpenDownloadSizeConfirmPopup(double fileSize, string sizeLabel)
        {
            popupManager.OpenPopup(new DownloadSizeConfPopupContext { FileSize = fileSize, Size = sizeLabel });
            var downloadSizeConfPopup = popupManager.CurrentOpenPopup as DownloadSizeConfPopup;
            await UniTask.WaitUntil(() => downloadSizeConfPopup.IsClose);
            return downloadSizeConfPopup.IsConfirm;
        }

        public static async UniTask<bool> TryOpenNetworkErrorPopup(ResponseBase response)
        {
            var result = await OpenNetworkErrorPopup(response.ErrorMessage, response.Status);
            return result.EnumEquals(ECommonPopupTapKind.Negative);
        }
    }
}
