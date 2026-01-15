using Cysharp.Text;
using Intense.UI;
using UnityEngine;

namespace Element.UI
{
    public sealed class WebViewPopupContext : PopupContext
    {
        public string TitleText { get; init; }
        public string Url { get; init; }
    }

    public class WebViewPopup : PopupBase
    {
        [SerializeField] private WebViewObject webViewObject;

        private string url;

        public void Open(WebViewPopupContext context)
        {
            closeCallback = context.NegativeCallback;
            url = context.Url;

            title.SetTextFormat("{0}", context.TitleText);
            base.Open();
        }

        protected override void FinishOpnePopupScale()
        {
            base.FinishOpnePopupScale();
#if UNITY_EDITOR
            Application.OpenURL(url);
#elif UNITY_IPHONE || UNITY_ANDROID
            const int contWidth = 1280;
            const int contHeight = 720;
            const float webViewOffsetLeft = 226;
            const float webViewOffsetTop = 179;
            const float webViewOffsetRight = 222;
            const float webViewOffsetBottom = 189;

            var scale = (float)Screen.height / contHeight;

            var marginTop = (int)(webViewOffsetTop * scale);
            var marginBottom = (int)(webViewOffsetBottom * scale);
            var baseOffset = (Screen.width - Screen.height * ((float)contWidth / contHeight)) / 2;
            var marginLeft = (int)(baseOffset + webViewOffsetLeft * scale);
            var marginRight = (int)(baseOffset + webViewOffsetRight * scale);

            webViewObject.Init(cb: Debug.Log, transparent: true, enableWKWebView: true);
            webViewObject.LoadURL(url);
            webViewObject.SetMargins(marginLeft, marginTop, marginRight, marginBottom);
            webViewObject.SetVisibility(true);
#endif
        }

        protected override void FinishClosePopupScale()
        {
            base.FinishClosePopupScale();
            Destroy(gameObject);
        }
    }
}