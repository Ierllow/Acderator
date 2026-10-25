#nullable enable

using Intense.UI;
using R3;
using System;
using UnityEngine;

namespace Song
{
    public enum PopupTapKind { None = 0, Resume, Restart, Quit }

    public class PausePopup : PopupBase
    {
        [SerializeField] private CommonButton resumeButton = default!;
        [SerializeField] private CommonButton restartButton = default!;
        [SerializeField] private CommonButton quitButton = default!;

        public PopupTapKind TapKind { get; private set; }

        private void Start()
        {
            resumeButton.OnTapButtonAsObservable.SubscribeLock(_ => Close(PopupTapKind.Resume)).RegisterTo(destroyCancellationToken);
            restartButton.OnTapButtonAsObservable.SubscribeLock(_ => Close(PopupTapKind.Restart)).RegisterTo(destroyCancellationToken);
            quitButton.OnTapButtonAsObservable.SubscribeLock(_ => Close(PopupTapKind.Quit)).RegisterTo(destroyCancellationToken);
        }

        public void Open(bool showRestartButton, Action callback)
        {
            TapKind = PopupTapKind.None;
            restartButton.gameObject.SetActive(showRestartButton);
            base.Open(callback);
        }

        private void Close(PopupTapKind popupTapKind)
        {
            TapKind = popupTapKind;
            base.Close();
        }
    }
}