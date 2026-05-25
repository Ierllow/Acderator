using DG.Tweening;
using Intense.Data;
using Intense.UI;
using R3;
using TMPro;
using UnityEngine;
using Zenject;

namespace Song
{
    public class SongPlayingFieldView : MonoBehaviour
    {
        [SerializeField] private CommonButton pauseButton;
        [SerializeField] private CanvasGroup pauseButtonCanvasGroup;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private CanvasGroup autoText;

        public CommonButton PauseButton => pauseButton;

        [Inject] private ScoreController scoreController;

        private const float ScoreTweenDuration = 0.4f;
        private int displayedScore = 0;

        private void Start() => scoreController.CurrentScore.Subscribe(score => UpdateDisplay((int)score)).RegisterTo(destroyCancellationToken);

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

        private void UpdateDisplay(int currentScore) => DOTween.To(() => displayedScore, v => displayedScore = v, currentScore, ScoreTweenDuration).OnUpdate(() => SetScoreText(displayedScore)).SetLink(gameObject);

        private void SetScoreText(int score)
        {
            scoreText.enableVertexGradient = ScoreUtils.IsExc(score);
            scoreText.SetText("{0:D7}", score);
        }
    }
}