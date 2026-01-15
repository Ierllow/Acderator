using Cysharp.Threading.Tasks;
using DG.Tweening;
using Intense;
using Intense.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Song
{
    public class BackTelopLayerController : MonoBehaviour
    {
        [SerializeField] private AtlasImage backgroundImage;
        [SerializeField] private CanvasGroup songInfoCanvas;
        [SerializeField] private SongIntroView songIntroView;
        [SerializeField] private Image fadeImage;

        public async UniTask ShowSongIntro(SongInfo songInfo)
        {
            songIntroView.Show(songInfo);
            songInfoCanvas.alpha = 1f;

            await SceneManager.Instance.FadeInAsync();
            await songInfoCanvas.DOFade(0f, 2f).SetDelay(5f).WithCancellation(destroyCancellationToken);
            songInfoCanvas.gameObject.SetActive(false);
            await UniTask.NextFrame();
        }

        public void SetBackgroundImage(int bg) => backgroundImage.SetAtlas(bg.ToString(), "song/bg");

        public async UniTask FadeIn() => await fadeImage.DOFade(0.0f, 2f);

        public async UniTask FadeOut() => await fadeImage.DOFade(1.0f, 2f);
    }
}