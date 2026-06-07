#nullable enable

using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Song
{
    public class HpBar : MonoBehaviour
    {
        [SerializeField] private Image gaugeImage = default!;
        [SerializeField] private TextMeshProUGUI percent = default!;

        [Inject] private readonly HpBarController hpBarController = default!;

        private void Start() => hpBarController.CurrentHpPercent.Subscribe(currentHp =>
        {
            gaugeImage.fillAmount = currentHp;
            percent.SetText(((int)(currentHp * 100)).ToString());
        }).RegisterTo(destroyCancellationToken);

        public void Move() => gameObject.transform.DOMoveX(170, 0.6f).SetLink(gameObject);
    }
}
