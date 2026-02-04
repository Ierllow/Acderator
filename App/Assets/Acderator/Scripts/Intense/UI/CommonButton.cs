using Coffee.UIEffects;
using Cysharp.Text;
using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ColorMode = Coffee.UIEffects.ColorMode;

namespace Intense.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button), typeof(ButtonAnimation))]
    public class CommonButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] protected Button button;
        [SerializeField] protected TextMeshProUGUI buttonText;
        [SerializeField] protected UIEffect uIEffect;
        [SerializeField] protected float grayFactory = 0.4f;

        public virtual bool IsTapping { get; protected set; }

        public virtual Observable<Unit> OnTapButtonAsObservable => button.OnClickAsObservable();

        protected virtual void Awake()
        {
            if (uIEffect == null) return;

            if (!uIEffect.effectMode.EnumEquals(EffectMode.None)) uIEffect.effectMode = EffectMode.None;
            if (!uIEffect.colorMode.EnumEquals(ColorMode.Subtract)) uIEffect.colorMode = ColorMode.Subtract;
            if (!uIEffect.blurMode.EnumEquals(BlurMode.None)) uIEffect.blurMode = BlurMode.None;
        }

        public void OnPointerUp(PointerEventData eventData) => IsTapping = false;

        public void OnPointerDown(PointerEventData eventData) => IsTapping = true;

        public virtual void SetGrayOut(bool value)
        {
            if (uIEffect == null)
            {
                Debug.LogWarning(ZString.Format("{0} is null", uIEffect.GetType().Name));
                return;
            }
            uIEffect.colorFactor = value ? grayFactory : 0f;
            button.interactable = !value;
        }

        public virtual void SetButtonText(string text)
        {
            if (buttonText == null)
            {
                Debug.LogWarning(ZString.Format("{0} is null", buttonText.GetType().Name));
                return;
            }
            buttonText.SetText(text);
        }
    }
}