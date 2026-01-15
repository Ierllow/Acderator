using Cysharp.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Song
{
    public class HpBar : MonoBehaviour
    {
        [SerializeField] private Image gaugeImage;
        [SerializeField] private TextMeshProUGUI percent;

        public void SetHp(float currentHp)
        {
            gaugeImage.fillAmount = currentHp;
            percent.SetTextFormat("{0}", (int)(currentHp * 100));
        }

        public void Move() => gameObject.transform.DOMoveX(170, 0.6f).SetLink(gameObject);
    }
}