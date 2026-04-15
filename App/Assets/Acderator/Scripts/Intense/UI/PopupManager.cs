using Cysharp.Threading.Tasks.Linq;
using Element.UI;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Intense.UI
{
    public class PopupManager : IInitializable
    {
        private readonly List<PopupBase> openedPopupList = new();
        public PopupBase CurrentOpenPopup => openedPopupList.LastOrDefault();

        public void Initialize() => CurrentOpenPopup.OnDisableAsObservable().Subscribe(_ =>
        {
            if (openedPopupList.Remove(CurrentOpenPopup)) Debug.Log(string.Format("{0} is closed", CurrentOpenPopup));
            Debug.LogWarning(string.Format("{0} is not opened", CurrentOpenPopup));
        });

        public void OpenPopup<T>(T context) where T : PopupContext
        {
            PopupBase popupBase;
            var path = "Popup/{0}";
            switch (context)
            {
                case CommonPopupContext commonPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(string.Format(path, typeof(CommonPopup).Name)));
                    (popupBase as CommonPopup).Open(commonPopupContext);
                    break;
                case WebViewPopupContext webViewPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(string.Format(path, typeof(WebViewPopup).Name)));
                    (popupBase as WebViewPopup).Open(webViewPopupContext);
                    break;
                case MenuPopupContext menuPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(string.Format(path, typeof(MenuPopup).Name)));
                    (popupBase as MenuPopup).Open(menuPopupContext);
                    break;
                case ConfigPopupContext configPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(string.Format(path, typeof(ConfigPopup).Name)));
                    (popupBase as ConfigPopup).Open(configPopupContext);
                    break;
                case DownloadSizeConfPopupContext downloadSizeConfPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(string.Format(path, typeof(DownloadSizeConfPopup).Name)));
                    (popupBase as DownloadSizeConfPopup).Open(downloadSizeConfPopupContext);
                    break;
                default:
                    throw new NotImplementedException(string.Format("PopupContext: {0}", typeof(PopupContext)));
            }
            openedPopupList.Add(popupBase);

            Debug.Log(string.Format("{0} is open", CurrentOpenPopup));
        }
    }
}