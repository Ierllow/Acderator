using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Intense.UI;
using TMPro;
using UnityEngine;

namespace Element.UI
{
    public sealed class DownloadSizeConfPopupContext : PopupContext
    {
        public double FileSize { get; init; }
        public string Size { get; init; }
    }

    public class DownloadSizeConfPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI text;

        public bool IsConfirm { get; private set; } = false;
        public bool IsClose { get; private set; } = false;

        public void Open(DownloadSizeConfPopupContext context) => UniTask.Void(async () =>
        {
            Init();
            text.SetTextFormat("ゲームデータのダウンロードを行います \nよろしいですか(サイズ{0}{1})", context.FileSize, context.Size);
            base.Open(closeCallback);
            await UniTask.WaitUntil(() => IsClose, cancellationToken: destroyCancellationToken);
            base.Close();
        });

        public void OnTapDownload()
        {
            IsConfirm = true;
            IsClose = true;
        }

        public void OnTapCancel() => IsClose = true;

        public void Init()
        {
            IsConfirm = false;
            IsClose = false;
        }

        protected override void FinishClosePopupScale()
        {
            base.FinishClosePopupScale();
            Destroy(gameObject);
        }
    }
}