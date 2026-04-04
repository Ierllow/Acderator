using Cysharp.Text;
using Cysharp.Threading.Tasks.Linq;
using Element.UI;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ZLinq;

namespace Intense.UI
{
    public class PopupManager : IInitializable
    {
        private readonly List<PopupBase> openedPopupList = new();
        public PopupBase CurrentOpenPopup => openedPopupList.AsValueEnumerable().LastOrDefault();

        public void Initialize() => CurrentOpenPopup.OnDisableAsObservable().Subscribe(_ =>
        {
            if (openedPopupList.Remove(CurrentOpenPopup)) Debug.Log(ZString.Format("{0} is closed", CurrentOpenPopup));
            Debug.LogWarning(ZString.Format("{0} is not opened", CurrentOpenPopup));
        });

        public void OpenPopup<T>(T context) where T : PopupContext
        {
            PopupBase popupBase;
            var path = "Popup/{0}";
            switch (context)
            {
                case CommonPopupContext commonPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(CommonPopup).Name)));
                    (popupBase as CommonPopup).Open(commonPopupContext);
                    break;
                case WebViewPopupContext webViewPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(WebViewPopup).Name)));
                    (popupBase as WebViewPopup).Open(webViewPopupContext);
                    break;
                case MenuPopupContext menuPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(MenuPopup).Name)));
                    (popupBase as MenuPopup).Open(menuPopupContext);
                    break;
                case ConfigPopupContext configPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(ConfigPopup).Name)));
                    (popupBase as ConfigPopup).Open(configPopupContext);
                    break;
                case DownloadSizeConfPopupContext downloadSizeConfPopupContext:
                    popupBase = UnityEngine.Object.Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(DownloadSizeConfPopup).Name)));
                    (popupBase as DownloadSizeConfPopup).Open(downloadSizeConfPopupContext);
                    break;
                default:
                    throw new NotImplementedException(ZString.Format("PopupContext: {0}", typeof(PopupContext)));
            }
            openedPopupList.Add(popupBase);

            Debug.Log(ZString.Format("{0} is open", CurrentOpenPopup));
        }
    }
}