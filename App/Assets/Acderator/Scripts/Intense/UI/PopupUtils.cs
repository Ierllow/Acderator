using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.Api;
using Intense.Asset;
using System;

namespace Intense.UI
{
    public static class PopupUtils
    {
        public static async UniTask OpenErrorPopup()
        {
            var completionSource = AutoResetUniTaskCompletionSource.Create();
            PopupManager.Instance.OpenPopup(PopupContextFactory.CreateErrorPopupContext(completionSource));
            await completionSource.Task;
        }

        public static async UniTask<ECommonPopupTapKind> OpenNetworkErrorPopup(string error, int errorCode)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            PopupManager.Instance.OpenPopup(PopupContextFactory.CreateNetworkErrorPopupContext(completionSource, error, errorCode));
            return await completionSource.Task;
        }

        public static async UniTask<ECommonPopupTapKind> OpenAssetErrorPopup(EAssetBundleErrorKind kind, bool titleBake = false)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            PopupManager.Instance.OpenPopup(PopupContextFactory.CreateAssetErrorPopupContext(completionSource, kind, titleBake));
            return await completionSource.Task;
        }

        public static async UniTask<bool> TryOpenNetworkErrorPopup(ResponseBase response)
        {
            var result = await OpenNetworkErrorPopup(response.ErrorMessage, response.Status);
            return result.EnumEquals(ECommonPopupTapKind.Negative);
        }
    }
}