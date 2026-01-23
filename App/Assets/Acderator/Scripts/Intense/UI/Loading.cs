using Cysharp.Text;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Intense.UI
{
    public class Loading : SingletonMonoBehaviour<Loading>
    {
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private GameObject loadingRoot;
        [SerializeField] private GameObject progressBar;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI percent;
        [SerializeField] private EventSystem eventSystem;

        private Sequence sequence;
        private DOTweenTMPAnimator doTweenTMPAnimator;

        private bool isInit = false;

        public void ShowLoading()
        {
            if (loadingRoot.activeSelf) return;

            loadingRoot.SetActive(true);

            if (isInit && !sequence.IsPlaying())
            {
                sequence.Restart();
            }
            else
            {
                loadingText.DOFade(0, 0);
                sequence ??= DOTween.Sequence();
                doTweenTMPAnimator ??= new(loadingText);
                for (var i = 0; i < doTweenTMPAnimator.textInfo.characterCount; ++i)
                {
                    doTweenTMPAnimator.DOScaleChar(i, 0.7f, 0);
                    sequence.Append(doTweenTMPAnimator.DOOffsetChar(i, doTweenTMPAnimator.GetCharOffset(i) + new Vector3(0, 30, 0), 0.25f).SetEase(Ease.OutFlash, 2));
                    sequence.Join(doTweenTMPAnimator.DOFadeChar(i, 1, 0.25f));
                    sequence.Join(doTweenTMPAnimator.DOScaleChar(i, 1, 0.25f).SetEase(Ease.OutBack));
                    sequence.SetLoops(-1, LoopType.Restart);
                }
                sequence.Play();
                isInit = true;
            }
            eventSystem.enabled = false;
        }

        public void SetDownloadFileSize(long downloadedFileSize, long allFileSize)
        {
            if (allFileSize <= 0) return;
            if (!progressBar.activeSelf) progressBar.SetActive(true);

            var val = (int)Math.Round((double)downloadedFileSize / allFileSize * 100);
            slider.DOValue(val, 0.1f).OnUpdate(() => percent.SetTextFormat("{0}%", (int)slider.value));
        }

        public void ClearProgressBar()
        {
            progressBar.SetActive(false);
            percent.SetText("");
            slider.value = 0;
            HideLoading();
        }

        public void HideLoading()
        {
            sequence.Pause();
            loadingRoot.SetActive(false);
            eventSystem.enabled = true;
        }
    }
}