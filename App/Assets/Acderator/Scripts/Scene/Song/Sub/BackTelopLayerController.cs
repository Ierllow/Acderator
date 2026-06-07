#nullable enable

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Intense.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Song
{
    public class BackTelopLayerController : MonoBehaviour
    {
        [SerializeField] private AtlasImage backgroundImage = default!;
        [SerializeField] private CanvasGroup songInfoCanvas = default!;
        [SerializeField] private SongIntroView songIntroView = default!;
        [SerializeField] private Image fadeImage = default!;

        public async UniTask ShowSongIntro(SongInfo songInfo, Sprite jacket, Sprite difficulty)
        {
            songIntroView.Show(songInfo, jacket, difficulty);
            songInfoCanvas.alpha = 1f;

            await songInfoCanvas.DOFade(0f, 2f).SetDelay(5f).WithCancellation(destroyCancellationToken);
            songInfoCanvas.gameObject.SetActive(false);
            await UniTask.NextFrame();
        }

        public void SetBackgroundImage(Sprite background) => backgroundImage.SetSprite(background);

        public async UniTask FadeIn() => await fadeImage.DOFade(0.0f, 2f);

        public async UniTask FadeOut() => await fadeImage.DOFade(1.0f, 2f);
    }
}
