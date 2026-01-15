using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Element.UI;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace Intense.UI
{
    public class PopupManager : SingletonMonoBehaviour<PopupManager>
    {
        private readonly List<PopupBase> openedPopupList = new();
        public PopupBase CurrentOpenPopup => openedPopupList.AsValueEnumerable().LastOrDefault();

        private void Start() => CurrentOpenPopup.OnDisableAsObservable().Subscribe(_ =>
        {
            Debug.Log(ZString.Format("{0} is closed", CurrentOpenPopup));
            openedPopupList.Remove(CurrentOpenPopup);
        }).RegisterTo(destroyCancellationToken);

        public void OpenPopup<T>(T context) where T : PopupContext
        {
            PopupBase popupBase;
            var path = "Popup/{0}";
            switch (context)
            {
                case CommonPopupContext commonPopupContext:
                    popupBase = Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(CommonPopup).Name)));
                    (popupBase as CommonPopup).Open(commonPopupContext);
                    break;
                case WebViewPopupContext webViewPopupContext:
                    popupBase = Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(WebViewPopup).Name)));
                    (popupBase as WebViewPopup).Open(webViewPopupContext);
                    break;
                case MenuPopupContext menuPopupContext:
                    popupBase = Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(MenuPopup).Name)));
                    (popupBase as MenuPopup).Open(menuPopupContext);
                    break;
                case ConfigPopupContext configPopupContext:
                    popupBase = Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(ConfigPopup).Name)));
                    (popupBase as ConfigPopup).Open(configPopupContext);
                    break;
                case DownloadSizeConfPopupContext downloadSizeConfPopupContext:
                    popupBase = Instantiate(Resources.Load<PopupBase>(ZString.Format(path, typeof(DownloadSizeConfPopup).Name)));
                    (popupBase as DownloadSizeConfPopup).Open(downloadSizeConfPopupContext);
                    break;
                default:
                    throw new NotImplementedException(ZString.Format("PopupContext: {0}", typeof(PopupContext)));
            }
            openedPopupList.Add(popupBase);

            Debug.Log(ZString.Format("{0} is open", CurrentOpenPopup));
        }
    }

    static class Ex
    {
        public static async UniTask OpenErrorPopup(this AutoResetUniTaskCompletionSource completionSource)
        {
            TouchControlManager.Instance.SetEventSystemEnabled(false);
            PopupManager.Instance.OpenPopup(PopupContextFactory.CreateErrorPopupContext(completionSource));
            await completionSource.Task;
        }
    }
}