using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Intense;
using Intense.UI;
using System;
using TMPro;
using UnityEngine;
using uPalette.Generated;
using uPalette.Runtime.Core;

namespace Song
{
    public class FrontTelopLayerController : MonoBehaviour
    {
        [SerializeField] private GameObject missMask;
        [SerializeField] private GameObject countDownRoot;
        [SerializeField] private AtlasImage sliderRing;
        [SerializeField] private TextMeshProUGUI restTimeText;
        [SerializeField] private TextMeshProUGUI resultText;

        public void ShowMissMask() => UniTask.Void(async () =>
        {
            missMask.SetActive(true);
            await UniTask.Delay(10, cancellationToken: destroyCancellationToken);
            missMask.SetActive(false);
        });

        public async UniTask CountDownStart()
        {
            restTimeText.SetTextFormat("{0}", 3);
            countDownRoot.SetActive(true);
            TouchControlManager.Instance.SetEventSystemEnabled(true);
            await UniTask.WhenAll(
                sliderRing.DOFillAmount(1, 1).SetLoops(3, LoopType.Restart).WithCancellation(destroyCancellationToken),
                restTimeText.DOCounter(3, 0, 3).SetEase(Ease.Linear).SetDelay(0.5f).WithCancellation(destroyCancellationToken));
            sliderRing.fillAmount = 0;
            countDownRoot.SetActive(false);
            TouchControlManager.Instance.SetEventSystemEnabled(false);
        }

        public async UniTask ShowResult(ESongResultType resultType)
        {
            resultText.SetTextFormat("{0}", resultType);

            var gradient = PaletteStore.Instance.GradientPalette.GetActiveValue((resultType switch
            {
                ESongResultType.Excellent => GradientEntry.Exc,
                ESongResultType.FullCombo or ESongResultType.Clear => GradientEntry.Clear,
                ESongResultType.Failed => GradientEntry.Failed,
                _ => throw new NotImplementedException(),
            }).ToEntryId()).Value;
            resultText.colorGradient = new VertexGradient(gradient.Evaluate(0), gradient.Evaluate(0.33f), gradient.Evaluate(0.66f), gradient.Evaluate(1));
            var sequence = DOTween.Sequence();
            sequence = sequence.Append(resultText.DOFade(1, 1).SetEase(Ease.InOutFlash));
            sequence = sequence.Join(resultText.rectTransform.DOLocalMoveY(0, 1).SetEase(Ease.InOutFlash));
            await sequence.WithCancellation(destroyCancellationToken);
            DOTween.To(() => resultText.characterSpacing, x => resultText.characterSpacing = x, 10.0f, 5f).ToUniTask().Forget();
        }
    }
}