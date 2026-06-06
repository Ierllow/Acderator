using Cysharp.Threading.Tasks;
using Element.UI;
using System;

namespace Intense.UI
{
    public static class PopupContextFactory
    {
        public static CommonPopupContext CreateAssetErrorPopupContext(
            AutoResetUniTaskCompletionSource<ECommonPopupTapKind> completionSource,
            Action<PopupContext> openPopup = null,
            Func<UniTask> moveTitleScene = null) => new()
            {
                Title = "エラー",
                Text = "エラーが発生しました。\n 再度実行しますか。",
                PositiveText = "リトライ",
                NegativeText = "キャンセル",
                PositiveCallback = () => completionSource.TrySetResult(ECommonPopupTapKind.Positive),
                NegativeCallback = () =>
                {
                    if (moveTitleScene == null)
                    {
                        openPopup(new CommonPopupContext
                        {
                            Title = "確認",
                            Text = "タイトルに戻ります。",
                            NegativeText = "OK",
                            NegativeCallback = async () =>
                            {
                                completionSource.TrySetResult(ECommonPopupTapKind.Negative);
                                await moveTitleScene();
                            }
                        });
                        return;
                    }
                    completionSource.TrySetResult(ECommonPopupTapKind.Negative);
                }
            };

        public static CommonPopupContext CreateNetworkErrorPopupContext(AutoResetUniTaskCompletionSource<ECommonPopupTapKind> completionSource, string error, int errorCode) => new()
        {
            Title = "エラー",
            Text = error + "\n エラーコード:" + errorCode,
            PositiveText = "リトライ",
            NegativeText = "閉じる",
            PositiveCallback = () => completionSource.TrySetResult(ECommonPopupTapKind.Positive),
            NegativeCallback = () => completionSource.TrySetResult(ECommonPopupTapKind.Negative)
        };

        public static CommonPopupContext CreateErrorPopupContext(AutoResetUniTaskCompletionSource completionSource, Func<UniTask> moveTitleScene) => new()
        {
            Title = "エラー",
            Text = "予期せぬエラーが発生しました。\n タイトルに戻ります。",
            NegativeText = "OK",
            NegativeCallback = async () =>
            {
                completionSource.TrySetResult();
                await moveTitleScene();
            },
            ButtonType = EButtonType.Close,
        };

        public static DownloadSizeConfPopupContext CreateDownloadSizeConfirmPopupContext(double fileSize, string size) => new() { FileSize = fileSize, Size = size };
    }
}