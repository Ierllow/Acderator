using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.Api;
using Intense.Asset;
using PlayFab;

namespace Intense.UI
{
    public static class PopupContextFactory
    {
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
                    PopupManager.Instance.OpenPopup(new CommonPopupContext
                    {
                        Title = "確認",
                        Text = "タイトルに戻ります。",
                        NegativeText = "OK",
                        NegativeCallback = async () =>
                        {
                            completionSource.TrySetResult(ECommonPopupTapKind.Negative);
                            await SceneManager.Instance.ChangeSceneAsync(ESceneType.Title);
                        }
                    });
                    return;
                }
                completionSource.TrySetResult(ECommonPopupTapKind.Negative);
            }
        };

        public static CommonPopupContext CreateNetworkErrorPopupContext(AutoResetUniTaskCompletionSource<ECommonPopupTapKind> completionSource, PlayFabError error) => new()
        {
            Title = "エラー",
            Text = error.Error.GetErrorMessage() + "\n エラーコード:" + "" + error.HttpCode.ToString(),
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
                await SceneManager.Instance.ChangeSceneAsync(ESceneType.Title);
            },
            ButtonType = EButtonType.Close,
        };

    }
}