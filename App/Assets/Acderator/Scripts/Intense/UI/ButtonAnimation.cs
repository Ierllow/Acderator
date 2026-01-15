using DG.Tweening;
using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Intense.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(Button))]
    public class ButtonAnimation : MonoBehaviour
    {
        private void Start() => this.OnPointerUpDownAsObservable().Subscribe(type => transform.DOScale(type.EnumEquals(R3.Triggers.PointerType.Down) ? new Vector3(0.9f, 0.9f, 1.0f) : Vector3.one, 0.1f).SetLink(gameObject)).RegisterTo(destroyCancellationToken);
    }
}