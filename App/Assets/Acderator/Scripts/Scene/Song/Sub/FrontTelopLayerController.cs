using Cysharp.Threading.Tasks;
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

        public void ShowMissMask() => UniTask.Void(async () => await UniTask.Delay(10, cancellationToken: destroyCancellationToken).SetAutoInActive(missMask));

        public async UniTask CountDownStart() => await countDownBar.CountDown().SetAutoInActive(countDownBar.gameObject, inputBlocker);

        public async UniTask ShowResult(ESongResultType resultType)
        {
            resultText.SetText(resultType.ToString());

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

    static class GameObjectExtensions
    {
        public static async UniTask SetAutoInActive(this UniTask task, GameObject go)
        {
            go.SetActive(true);
            await task;
            go.SetActive(false);
        }

        public static async UniTask SetAutoInActive(this UniTask task, GameObject go, Image image)
        {
            go.SetActive(true);
            image.raycastTarget = false;
            await task;
            image.raycastTarget = true;
            go.SetActive(false);
        }
    }
}