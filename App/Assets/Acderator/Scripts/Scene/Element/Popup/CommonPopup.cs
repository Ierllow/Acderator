using Cysharp.Text;
using Intense.UI;
using System;
using TMPro;
using UnityEngine;

namespace Element.UI
{
    public enum ECommonPopupTapKind { None = 0, Positive, Negative }
    public enum EButtonType { Close, Both }

    public sealed class CommonPopupContext : PopupContext
    {
        public string Title { get; init; }
        public string Text { get; init; }
        public string PositiveText { get; init; }
        public string NegativeText { get; init; }
        public Action PositiveCallback { get; init; }
        public EButtonType ButtonType { get; init; }
    }

    public class CommonPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUIEx text;
        [SerializeField] private TextMeshProUGUI positiveText;
        [SerializeField] private TextMeshProUGUI negativeText;
        [SerializeField] private GameObject positiveObj;

        private Action positiveCallback;

        public void Open(CommonPopupContext context)
        {
            positiveCallback = context.PositiveCallback;
            closeCallback = context.NegativeCallback;

            title.SetTextFormat("{0}", context.Title);
            text.Text.SetTextFormat("{0}", context.Text);
            negativeText.SetTextFormat("{0}", context.NegativeText);

            var isBoth = context.ButtonType.EnumEquals(EButtonType.Both);
            positiveObj.SetActive(isBoth);
            positiveText.SetTextFormat("{0}", isBoth ? context.PositiveText : "");

            base.Open(closeCallback);
        }

        public void OnTapPositiveButton()
        {
            positiveCallback?.Invoke();
            Close();
        }

        protected override void FinishClosePopupScale()
        {
            base.FinishClosePopupScale();
            Destroy(gameObject);
        }
    }
}