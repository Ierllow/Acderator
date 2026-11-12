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
            BgmNum.SetText(FormatPercent(BgmSlider.value));
            SeSlider.value = PlayerPrefsValues.SeVolume;
            SeNum.SetText(FormatPercent(SeSlider.value));
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
                    UpdateBgmVolume(BgmSlider.value);
                    break;
                case ConfigType.Se:
                    UpdateSeVolume(SeSlider.value);
                    break;
                default:
                    break;
            }
        }

        private void UpdateBgmVolume(float volume)
        {
            var isMuted = volume == 0;
            PlayerPrefsValues.Set(PlayerPrefsKey.BgmVolume, volume);
            PlayerPrefsValues.Set(PlayerPrefsKey.BgmMute, isMuted);
            PlayerPrefsValues.Set(PlayerPrefsKey.SongVolume, volume);
            PlayerPrefsValues.Set(PlayerPrefsKey.SongMute, isMuted);
            bgmVolumeController.UpdateVolume(volume, isMuted);
            songVolumeController.UpdateVolume(volume, isMuted);
            BgmNum.SetText(FormatPercent(volume));
        }

        private void UpdateSeVolume(float volume)
        {
            var isMuted = volume == 0;
            PlayerPrefsValues.Set(PlayerPrefsKey.SeVolume, volume);
            PlayerPrefsValues.Set(PlayerPrefsKey.SeMute, isMuted);
            seVolumeController.UpdateVolume(volume, isMuted);
            SeNum.SetText(FormatPercent(volume));
        }

        private string FormatPercent(float value) => string.Format("{0}%", Math.Round(value * 100));
    }
}