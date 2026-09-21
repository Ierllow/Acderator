using Coffee.UIEffects;
using ColorMode = Coffee.UIEffects.ColorMode;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Intense.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button), typeof(ButtonAnimation))]
    public class CommonButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] protected Button button;
        [SerializeField] protected TextMeshProUGUI buttonText;
        [FormerlySerializedAs("uIEffect"), SerializeField] protected UIEffect uiEffect;
        [FormerlySerializedAs("grayFactory"), SerializeField] protected float grayscaleFactor = 0.4f;

        public virtual bool IsTapping { get; protected set; }

        public virtual Observable<Unit> OnTapButtonAsObservable => button.OnClickAsObservable();

        protected virtual void Awake()
        {
            if (uiEffect == null) return;
            if (uiEffect.effectMode != EffectMode.None) uiEffect.effectMode = EffectMode.None;
            if (uiEffect.colorMode != ColorMode.Subtract) uiEffect.colorMode = ColorMode.Subtract;
            if (uiEffect.blurMode != BlurMode.None) uiEffect.blurMode = BlurMode.None;
        }

        public void OnPointerUp(PointerEventData eventData) => IsTapping = false;

        public void OnPointerDown(PointerEventData eventData) => IsTapping = true;

        public virtual void SetGrayOut(bool value)
        {
            if (uiEffect == null)
            {
                Debug.LogWarning(string.Format("{0} is not assigned", nameof(uiEffect)), this);
                return;
            }
            uiEffect.colorFactor = value ? grayscaleFactor : 0f;
            button.interactable = !value;
        }

        public virtual void SetButtonText(string text)
        {
            if (buttonText == null)
            {
                Debug.LogWarning(string.Format("{0} is not assigned", nameof(buttonText)), this);
                return;
            }
            buttonText.SetText(text);
        }
    }
}