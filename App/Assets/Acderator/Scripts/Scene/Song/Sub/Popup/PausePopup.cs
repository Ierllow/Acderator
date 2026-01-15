using Intense.UI;
using R3;
using System;
using UnityEngine;

namespace Song
{
    public enum EPopupTapKind { None = 0, Resume, Restart, Quit }

    public class PausePopup : PopupBase
    {
        [SerializeField] private CommonButton resumeButton;
        [SerializeField] private CommonButton restartButton;
        [SerializeField] private CommonButton quitButton;

        public EPopupTapKind TapKind { get; private set; }

        private void Start()
        {
            resumeButton.OnTapButtonAsObservable.SubscribeLock(new(true), _ => Close(EPopupTapKind.Resume)).RegisterTo(destroyCancellationToken);
            restartButton.OnTapButtonAsObservable.SubscribeLock(new(true), _ => Close(EPopupTapKind.Restart)).RegisterTo(destroyCancellationToken);
            quitButton.OnTapButtonAsObservable.SubscribeLock(new(true), _ => Close(EPopupTapKind.Quit)).RegisterTo(destroyCancellationToken);
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