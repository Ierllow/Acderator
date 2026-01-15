using Cysharp.Text;
using DG.Tweening;
using Intense.Data;
using Intense.UI;
using TMPro;
using UnityEngine;

namespace Song
{
    public class SongPlayingFieldView : MonoBehaviour
    {
        [SerializeField] private CommonButton pauseButton;
        [SerializeField] private CanvasGroup pauseButtonCanvasGroup;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private CanvasGroup autoText;

        public CommonButton PauseButton => pauseButton;

        private readonly ScoreNumController scoreNumController = new();

        public void Setup()
        {
            pauseButtonCanvasGroup.DOFade(1, 0.4f).SetLink(gameObject);
            scoreText.transform.DOLocalMoveX(369, 0.6f).SetLink(gameObject);
        }

        public void SetAutoTextActive(bool isValue)
        {
            autoText.gameObject.SetActive(isValue);
            if (isValue) autoText.DOFade(0, 1).SetEase(Ease.Flash, 1).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }

        public void UpdateDisplay(int currentScore) => scoreNumController.OnUpdate(currentScore, (score) =>
        {
            scoreText.enableVertexGradient = ScoreUtils.IsExc(score);
            scoreText.SetTextFormat("{0:D7}", score);
        });
    }
}