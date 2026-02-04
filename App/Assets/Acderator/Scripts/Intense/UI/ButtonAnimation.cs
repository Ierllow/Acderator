using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Intense.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(Button))]
    public class ButtonAnimation : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public void OnPointerUp(PointerEventData eventData) => transform.DOScale(Vector3.one, 0.1f).SetLink(gameObject);
        public void OnPointerDown(PointerEventData eventData) => transform.DOScale(new Vector3(0.9f, 0.9f, 1.0f), 0.1f).SetLink(gameObject);
    }
}