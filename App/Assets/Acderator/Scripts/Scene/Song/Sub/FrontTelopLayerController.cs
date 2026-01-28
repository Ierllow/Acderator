using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using uPalette.Generated;
using uPalette.Runtime.Core;

namespace Song
{
    public class FrontTelopLayerController : MonoBehaviour
    {
        [SerializeField] private GameObject missMask;
        [SerializeField] private CountDownBar countDownBar;
        [SerializeField] private Image inputBlocker;
        [SerializeField] private TextMeshProUGUI resultText;

        public void ShowMissMask() => UniTask.Void(async () =>
        {
            missMask.SetActive(true);
            await UniTask.Delay(10, cancellationToken: destroyCancellationToken);
            missMask.SetActive(false);
        });

        public async UniTask CountDownStart()
        {
            countDownBar.gameObject.SetActive(true);
            inputBlocker.raycastTarget = false;
            await countDownBar.CountDown();
            countDownBar.gameObject.SetActive(false);
            inputBlocker.raycastTarget = true;
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