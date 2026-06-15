#nullable enable

using Intense.UI;
using R3;
using System;
using UnityEngine;

namespace Song
{
    public enum EPopupTapKind { None = 0, Resume, Restart, Quit }

    public class PausePopup : PopupBase
    {
        [SerializeField] private CommonButton resumeButton = default!;
        [SerializeField] private CommonButton restartButton = default!;
        [SerializeField] private CommonButton quitButton = default!;

        public EPopupTapKind TapKind { get; private set; }

        private void Start()
        {
            resumeButton.OnTapButtonAsObservable.SubscribeLock(_ => Close(EPopupTapKind.Resume)).RegisterTo(destroyCancellationToken);
            restartButton.OnTapButtonAsObservable.SubscribeLock(_ => Close(EPopupTapKind.Restart)).RegisterTo(destroyCancellationToken);
            quitButton.OnTapButtonAsObservable.SubscribeLock(_ => Close(EPopupTapKind.Quit)).RegisterTo(destroyCancellationToken);
        }

        public void Open(bool showRestartButton, Action callback)
        {
            TapKind = EPopupTapKind.None;
            restartButton.gameObject.SetActive(showRestartButton);
            base.Open(callback);
        }

        private void Close(EPopupTapKind popupTapKind)
        {
            TapKind = popupTapKind;
            base.Close();
        }
    }
}