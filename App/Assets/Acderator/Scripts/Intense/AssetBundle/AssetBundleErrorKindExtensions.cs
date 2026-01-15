using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.UI;
using System;

namespace Intense.Asset
{
    public static class AssetBundleErrorKindExtensions
    {
        public static string GetErrorMessage(this EAssetBundleErrorKind kind) => kind switch
        {
            EAssetBundleErrorKind.Error or EAssetBundleErrorKind.NotFoundManifest => "エラーが発生しました。",
            EAssetBundleErrorKind.ConnectionError => "通信環境をご確認ください。",
            EAssetBundleErrorKind.ProtocolError => "通信に失敗しました。",
            _ => "",
        };

        public static bool IsFailed(this EAssetBundleErrorKind kind) =>
            kind.EnumEquals(EAssetBundleErrorKind.ProtocolError)
                || kind.EnumEquals(EAssetBundleErrorKind.ConnectionError)
                || kind.EnumEquals(EAssetBundleErrorKind.Error);

        public static bool IsCanceled(this EAssetBundleErrorKind kind) => kind.EnumEquals(EAssetBundleErrorKind.Canceled);

        public static async UniTask<bool> OnOpenAssetPopup(this EAssetBundleErrorKind kind)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            PopupManager.Instance.OpenPopup(PopupContextFactory.CreateAssetErrorPopupContext(completionSource, kind));
            return (await completionSource.Task).EnumEquals(ECommonPopupTapKind.Negative);
        }
    }
}