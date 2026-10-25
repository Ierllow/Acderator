using Intense;
using Intense.UI;
using Song;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Element.UI
{
    public sealed class ConfigPopupContext : PopupContext<ConfigPopup> { }

    public class ConfigPopup : PopupBase<ConfigPopupContext>
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

        [Inject] private readonly BgmVolumeController bgmVolumeController;
        [Inject] private readonly SeVolumeController seVolumeController;
        [Inject] private readonly SongVolumeController songVolumeController;

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

        public override void Open(ConfigPopupContext configPopupContext)
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
                    PlayerPrefsValues.Set(PlayerPrefsKey.SongVolume, BgmSlider.value);
                    PlayerPrefsValues.Set(PlayerPrefsKey.SongMute, BgmSlider.value == 0);
                    bgmVolumeController.UpdateVolume(BgmSlider.value, BgmSlider.value == 0);
                    songVolumeController.UpdateVolume(BgmSlider.value, BgmSlider.value == 0);
                    BgmNum.SetText(string.Format("{0}%", Math.Round(BgmSlider.value * 100)));
                    break;
                case ConfigType.Se:
                    PlayerPrefsValues.Set(PlayerPrefsKey.SeVolume, SeSlider.value);
                    PlayerPrefsValues.Set(PlayerPrefsKey.SeMute, SeSlider.value == 0);
                    seVolumeController.UpdateVolume(SeSlider.value, SeSlider.value == 0);
                    SeNum.SetText(string.Format("{0}%", Math.Round(SeSlider.value * 100)));
                    break;
                default:
                    break;
            }
        }
    }
}