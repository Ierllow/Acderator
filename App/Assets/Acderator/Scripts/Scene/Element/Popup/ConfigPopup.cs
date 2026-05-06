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
            noteSppedSlider.value = PlayerPrefsValues.NoteSpeed / 10;
            noteSpeedNum.SetText(Math.Round(noteSppedSlider.value * 10).ToString());
            offsetSlider.value = PlayerPrefsValues.TapTiming;
            tapTimingNum.SetText(Math.Round(offsetSlider.value * 10).ToString());
            BgmSlider.value = PlayerPrefsValues.BgmVolume;
            BgmNum.SetText(string.Format("{0}%", Math.Round(BgmSlider.value * 100)));
            SeSlider.value = PlayerPrefsValues.SeVolume;
            SeNum.SetText(string.Format("{0}%", Math.Round(SeSlider.value * 100)));
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
                    PlayerPrefsValues.Set(PlayerPrefsKey.NoteSpeedConfig, noteSppedSlider.value * 10);
                    noteSpeedNum.SetText(Math.Round(noteSppedSlider.value * 10).ToString());
                    break;
                case ConfigType.Offset:
                    PlayerPrefsValues.Set(PlayerPrefsKey.TapTimingNum, offsetSlider.value);
                    tapTimingNum.SetText(Math.Round(offsetSlider.value * 10).ToString());
                    break;
                case ConfigType.Bgm:
                    PlayerPrefsValues.Set(PlayerPrefsKey.BgmVolume, BgmSlider.value);
                    PlayerPrefsValues.Set(PlayerPrefsKey.BgmMute, BgmSlider.value == 0);
                    soundManager.UpdateBgmVolume(BgmSlider.value, BgmSlider.value == 0);
                    soundManager.UpdateSongVolume(SeSlider.value, SeSlider.value == 0);
                    BgmNum.SetText(string.Format("{0}%", Math.Round(BgmSlider.value * 100)));
                    break;
                case ConfigType.Se:
                    PlayerPrefsValues.Set(PlayerPrefsKey.SeVolume, SeSlider.value);
                    PlayerPrefsValues.Set(PlayerPrefsKey.SeMute, SeSlider.value == 0);
                    soundManager.UpdateSeVolume(SeSlider.value, SeSlider.value == 0);
                    SeNum.SetText(string.Format("{0}%", Math.Round(SeSlider.value * 100)));
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