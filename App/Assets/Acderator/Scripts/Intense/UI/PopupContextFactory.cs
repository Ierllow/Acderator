using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.Asset;
using Zenject;

namespace Intense.UI
{
    public static class PopupContextFactory
    {
        [Inject] private static PopupManager popupManager;
        [Inject] private static SceneManager sceneManager;
        public static CommonPopupContext CreateAssetErrorPopupContext(AutoResetUniTaskCompletionSource<ECommonPopupTapKind> completionSource, EAssetBundleErrorKind kind, bool titleBake = false) => new()
        {
            Title = "エラー",
            Text = kind.GetErrorMessage() + "\n 再度実行しますか。",
            PositiveText = "リトライ",
            NegativeText = "キャンセル",
            PositiveCallback = () => completionSource.TrySetResult(ECommonPopupTapKind.Positive),
            NegativeCallback = () =>
            {
                if (titleBake)
                {
                    popupManager.OpenPopup(new CommonPopupContext
                    {
                        Title = "確認",
                        Text = "タイトルに戻ります。",
                        NegativeText = "OK",
                        NegativeCallback = async () =>
                        {
                            completionSource.TrySetResult(ECommonPopupTapKind.Negative);
                            await sceneManager.ChangeSceneAsync(ESceneType.Title);
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
            Text = error + "\n エラーコード:" + "" + errorCode.ToString(),
            PositiveText = "リトライ",
            NegativeText = "閉じる",
            PositiveCallback = () => completionSource.TrySetResult(ECommonPopupTapKind.Positive),
            NegativeCallback = () => completionSource.TrySetResult(ECommonPopupTapKind.Negative)
        };

        public static CommonPopupContext CreateErrorPopupContext(AutoResetUniTaskCompletionSource completionSource) => new()
        {
            Title = "エラー",
            Text = "予期せぬエラーが発生しました。\n タイトルに戻ります。",
            NegativeText = "OK",
            NegativeCallback = async () =>
            {
                completionSource.TrySetResult();
                await sceneManager.ChangeSceneAsync(ESceneType.Title);
            },
            ButtonType = EButtonType.Close,
        };
    }
}