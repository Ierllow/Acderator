using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.UI;
using Zenject;

namespace Intense.Asset
{
    internal sealed class AddressableAssetPopupController
    {
        [Inject] private readonly PopupManager popupManager;

        public async UniTask<bool> TryRetryAssetErrorAsync()
        {
            var completionSource = AutoResetUniTaskCompletionSource<CommonPopupTapKind>.Create();
            var popupContext = PopupContextFactory.CreateAssetErrorPopupContext(completionSource);
            popupManager.OpenPopup(popupContext);
            return await completionSource.Task == CommonPopupTapKind.Positive;
        }

        public async UniTask<bool> TryDownloadConfirmedAsync(long fileSize)
        {
            if (fileSize <= 0) return true;

            var (value, unit) = fileSize.ToDisplayFileSize();
            var context = PopupContextFactory.CreateDownloadSizeConfirmPopupContext(value, unit);
            var popup = popupManager.OpenPopup(context);
            await UniTask.WaitUntil(() => popup.IsClose);
            return popup.IsConfirm;
        }
    }
}