using Intense.UI;
using System;
using TMPro;
using UnityEngine;

namespace Element.UI
{
    public enum CommonPopupTapKind { None = 0, Positive, Negative }
    public enum PopupButtonType { Close, Both }

    public sealed class CommonPopupContext : PopupContext<CommonPopup>
    {
        public string Title { get; init; }
        public string Text { get; init; }
        public string PositiveText { get; init; }
        public string NegativeText { get; init; }
        public Action PositiveCallback { get; init; }
        public PopupButtonType ButtonType { get; init; }
    }

    public class CommonPopup : PopupBase<CommonPopupContext>
    {
        [SerializeField] private TextMeshProUGUIEx text;
        [SerializeField] private TextMeshProUGUI positiveText;
        [SerializeField] private TextMeshProUGUI negativeText;
        [SerializeField] private GameObject positiveObj;

        private Action positiveCallback;

        public override void Open(CommonPopupContext context)
        {
            positiveCallback = context.PositiveCallback;
            closeCallback = context.NegativeCallback;

            title.SetText(context.Title);
            text.Text.SetText(context.Text);
            negativeText.SetText(context.NegativeText);

            var isBoth =context.ButtonType == PopupButtonType.Both;
            positiveObj.SetActive(isBoth);
            positiveText.SetText(isBoth ? context.PositiveText : "");

            base.Open(closeCallback);
        }

        public void OnTapPositiveButton()
        {
            positiveCallback?.Invoke();
            Close();
        }
    }
}