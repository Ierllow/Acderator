using Cysharp.Threading.Tasks;
using Element.UI;
using Intense.UI;
using System;

namespace Intense
{
    public enum EFileSizeType
    {
        [Text("KB")] Kb,
        [Text("MB")] Mb,
        [Text("GB")] Gb,
    }

    public class DownloadSizeConfManager : SingletonMonoBehaviour<DownloadSizeConfManager>
    {
        public bool IsDownloadSizePopupNotRun { get; private set; }
        public bool IsProgressInactive { get; private set; }

        public async UniTask<bool> OnOpenDownloadSizeConfPopupAsync(long newFileSize)
        {
            var fileSize = GetFileSize(newFileSize);
            if (IsDownloadSizePopupNotRun && fileSize > 0)
            {
                PopupManager.Instance.OpenPopup(new DownloadSizeConfPopupContext { FileSize = fileSize, Size = GetFileSizeType(newFileSize).GetTextAttribute().Text });
                var downloadSizeConfPopup = PopupManager.Instance.CurrentOpenPopup as DownloadSizeConfPopup;
                await UniTask.WaitUntil(() => downloadSizeConfPopup.IsClose);
                return downloadSizeConfPopup.IsConfirm;
            }
            return false;
        }

        public void SetDownloadSizePopupNotRun() => IsDownloadSizePopupNotRun = true;

        public void ResetDownloadSizePopupNotRun() => IsDownloadSizePopupNotRun = false;

        public void SetProgressInActive() => IsProgressInactive = true;

        public void ResetProgressInActive() => IsProgressInactive = false;

        private EFileSizeType GetFileSizeType(long fileSize) => Math.Pow(1024, 2) > fileSize
            ? EFileSizeType.Kb : Math.Pow(1024, 3) > fileSize
            ? EFileSizeType.Mb : EFileSizeType.Gb;

        public double GetFileSize(long fileSize) => GetFileSizeType(fileSize) switch
        {
            EFileSizeType.Kb => Math.Round((double)fileSize / 1024, 2),
            EFileSizeType.Mb => Math.Round(fileSize / Math.Pow(1024, 2), 2),
            EFileSizeType.Gb => Math.Round(fileSize / Math.Pow(1024, 3), 2),
            _ => 0,
        };
    }
}