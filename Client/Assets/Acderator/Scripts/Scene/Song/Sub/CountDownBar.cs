#nullable enable

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Intense.UI;
using TMPro;
using UnityEngine;

namespace Song
{
    public class CountDownBar : MonoBehaviour
    {
        [SerializeField] private AtlasImage sliderRing = default!;
        [SerializeField] private TextMeshProUGUI restTimeText = default!;

        public async UniTask CountDown()
        {
            restTimeText.SetText("{0}", 3);
            await UniTask.WhenAll(
                sliderRing.DOFillAmount(1, 1).SetLoops(3, LoopType.Restart).WithCancellation(destroyCancellationToken),
                restTimeText.DOCounter(3, 0, 3).SetEase(Ease.Linear).SetDelay(0.5f).WithCancellation(destroyCancellationToken));
            sliderRing.fillAmount = 0;
        }
    }
}