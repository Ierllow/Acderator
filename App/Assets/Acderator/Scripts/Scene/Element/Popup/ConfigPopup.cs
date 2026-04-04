using Cysharp.Text;
using Intense;
using Intense.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Element.UI
{
    public sealed class ConfigPopupContext : PopupContext { }

    public class ConfigPopup : PopupBase
    {
        private enum ConfigType { NoteSpeed = 1, Offset, Bgm, Se }

        [SerializeField] private TextMeshProUGUI noteSpeedNum;
        [SerializeField] private TextMeshProUGUI tapTimingNum;
        [SerializeField] private TextMeshProUGUI BgmNum;
        [SerializeField] private TextMeshProUGUI SeNum;
        [SerializeField] private Slider noteSppedSlider;
        [SerializeField] private Slider offsetSlider;
        [SerializeField] private Slider BgmSlider;
        [SerializeField] private Slider SeSlider;

        [Inject] private SoundManager soundManager;

        private void Start()
        {
            noteSppedSlider.value = PlayerPrefsValues.NS / 10;
            noteSpeedNum.SetText(Math.Round(noteSppedSlider.value * 10));
            offsetSlider.value = PlayerPrefsValues.TN;
            tapTimingNum.SetText(Math.Round(offsetSlider.value * 10));
            BgmSlider.value = PlayerPrefsValues.BV;
            BgmNum.SetTextFormat("{0}%", Math.Round(BgmSlider.value * 100));
            SeSlider.value = PlayerPrefsValues.SV;
            SeNum.SetTextFormat("{0}%", Math.Round(SeSlider.value * 100));
        }

        public void Open(ConfigPopupContext configPopupContext)
        {
            closeCallback = configPopupContext.NegativeCallback;
            base.Open(closeCallback);
        }

        public void OnValueChange(int num)
        {
            switch ((ConfigType)num)
            {
                case ConfigType.NoteSpeed:
                    PlayerPrefsValues.Set(EKey.NoteSpeedConfig, noteSppedSlider.value * 10);
                    noteSpeedNum.SetText(Math.Round(noteSppedSlider.value * 10));
                    break;
                case ConfigType.Offset:
                    PlayerPrefsValues.Set(EKey.TapTimingNum, offsetSlider.value);
                    tapTimingNum.SetText(Math.Round(offsetSlider.value * 10));
                    break;
                case ConfigType.Bgm:
                    PlayerPrefsValues.Set(EKey.BgmVolume, BgmSlider.value);
                    PlayerPrefsValues.Set(EKey.BgmMute, BgmSlider.value == 0 ? 1 : 0);
                    soundManager.UpdateBgmVolume(BgmSlider.value, BgmSlider.value == 0);
                    soundManager.UpdateSongVolume(SeSlider.value, SeSlider.value == 0);
                    BgmNum.SetTextFormat("{0}%", Math.Round(BgmSlider.value * 100));
                    break;
                case ConfigType.Se:
                    PlayerPrefsValues.Set(EKey.SongVolume, SeSlider.value);
                    soundManager.UpdateSeVolume(BgmSlider.value, SeSlider.value == 0);
                    SeNum.SetTextFormat("{0}%", Math.Round(SeSlider.value * 100));
                    break;
                default:
                    break;
            }
        }

        protected override void FinishClosePopupScale()
        {
            base.FinishClosePopupScale();
            Destroy(gameObject);
        }
    }
}
