using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Intense.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class PopupBase : MonoBehaviour
    {
        [SerializeField] protected Animator popupAnimation;
        [SerializeField] protected TextMeshProUGUI title;

        protected Action openCallback;
        protected Action closeCallback;

        public virtual void Open(Action callback = null)
        {
            closeCallback = callback;
            gameObject.SetActive(true);
        }

        public virtual void Close() => popupAnimation.Play("close_popup");

        protected virtual void FinishOpnePopupScale()
        {
            openCallback?.Invoke();
            openCallback = null;
        }

        protected virtual void FinishClosePopupScale()
        {
            closeCallback?.Invoke();
            closeCallback = null;
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        protected enum PopupSize
        {
            S,
            M,
            L,
            LL,
            Custom = 999,
        }

        protected readonly List<(float, float)> contentSizeList = new()
        {
            (450f, 270),
            (800f, 500f),
            (1000f, 600f),
            (1200f, 700f),
        };

        protected readonly List<float> buttonRootPosList = new()
        {
            25f,
            35f,
            40f,
            45f,
        };

        protected readonly List<(float, float)> buttonSizeList = new()
        {
            (80f, 30f),
            (110f, 40f),
            (130f, 45f),
            (140f, 50f),
        };

        protected readonly List<float> buttonFontSizeList = new()
        {
            16f,
            24f,
            26f,
            28f,
        };

        protected readonly List<float> titleFontSizeList = new()
        {
            26f,
            30f,
            30f,
            32f,
        };

        [SerializeField] protected PopupSize popupSize = PopupSize.S;
        [SerializeField] protected RectTransform contentRect;
        [SerializeField] protected RectTransform buttonRect;

        [Button]
        protected virtual void SetContentSize()
        {
            if (popupSize == PopupSize.Custom) return;
            if (contentRect != null) contentRect.sizeDelta = new Vector2(contentSizeList[(int)popupSize].Item1, contentSizeList[(int)popupSize].Item2);
            if (buttonRect == null) return;

            Vector3 pos = buttonRect.anchoredPosition;
            buttonRect.anchoredPosition = new Vector3(pos.x, buttonRootPosList[(int)popupSize], pos.z);
            buttonRect.sizeDelta = new Vector2(buttonSizeList[(int)popupSize].Item1, buttonSizeList[(int)popupSize].Item2);

            var text = buttonRect.GetComponentInChildren<TextMeshProUGUI>();
            if (text.TryGetComponent<RectTransform>(out var textRect)) textRect.sizeDelta = new Vector2(buttonSizeList[(int)popupSize].Item1, buttonSizeList[(int)popupSize].Item2);
            if (text == null) return;

            text.fontSize = buttonFontSizeList[(int)popupSize];
            title.fontSize = titleFontSizeList[(int)popupSize];

        }
#endif
    }
}