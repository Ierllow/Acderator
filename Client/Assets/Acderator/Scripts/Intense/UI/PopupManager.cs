using System;
using System.Collections.Generic;
using UnityEngine;

namespace Intense.UI
{
    public class PopupManager
    {
        private const string PopupResourcePath = "Popup/{0}";

        private readonly Dictionary<Type, PopupBase> openedPopupDict = new();

        public TPopup OpenPopup<TPopup>(PopupContext<TPopup> context) where TPopup : PopupBase
        {
            if (openedPopupDict.TryGetValue(typeof(TPopup), out var openedPopup)) return (TPopup)openedPopup;

            var resourcePath = string.Format(PopupResourcePath, typeof(TPopup).Name);
            var popup = UnityEngine.Object.Instantiate(Resources.Load<TPopup>(resourcePath));
            openedPopupDict[typeof(TPopup)] = popup;
            popup.DestroyedCallback = (popup) => openedPopupDict.Remove(popup.GetType());
            popup.Open(context);
            Debug.Log(string.Format("{0} is open", popup));
            return popup;
        }
    }
}