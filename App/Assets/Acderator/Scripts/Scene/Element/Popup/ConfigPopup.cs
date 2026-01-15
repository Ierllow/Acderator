using Cysharp.Text;
using Intense;
using Intense.Data;
using Intense.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private void Start()
        {
            noteSppedSlider.value = LocalDataManager.Instance.Option.NoteSpeed / 10;
            noteSpeedNum.SetTextFormat("{0}", Math.Round(noteSppedSlider.value * 10));
            offsetSlider.value = LocalDataManager.Instance.Option.TapTimingNum;
            tapTimingNum.SetTextFormat("{0}", Math.Round(offsetSlider.value * 10));
            BgmSlider.value = LocalDataManager.Instance.Option.BgmVolume;
            BgmNum.SetTextFormat("{0}%", Math.Round(BgmSlider.value * 100));
            SeSlider.value = LocalDataManager.Instance.Option.SeVolume;
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
                    LocalDataManager.Instance.Option.NoteSpeed = noteSppedSlider.value * 10;
                    noteSpeedNum.SetTextFormat("{0}", Math.Round(noteSppedSlider.value * 10));
                    break;
                case ConfigType.Offset:
                    LocalDataManager.Instance.Option.TapTimingNum = offsetSlider.value;
                    tapTimingNum.SetTextFormat("{0}", Math.Round(offsetSlider.value * 10));
                    break;
                case ConfigType.Bgm:
                    LocalDataManager.Instance.Option.BgmVolume = BgmSlider.value;
                    LocalDataManager.Instance.Option.SongVolume = BgmSlider.value;
                    LocalDataManager.Instance.Option.IsMuteBgm = LocalDataManager.Instance.Option.BgmVolume == 0;
                    SoundManager.Instance.UpdateBgmVolume(LocalDataManager.Instance.Option.BgmVolume, LocalDataManager.Instance.Option.IsMuteBgm);
                    LocalDataManager.Instance.Option.IsMuteSong = LocalDataManager.Instance.Option.SongVolume == 0;
                    SoundManager.Instance.UpdateSongVolume(LocalDataManager.Instance.Option.SongVolume, LocalDataManager.Instance.Option.IsMuteSong);
                    BgmNum.SetTextFormat("{0}%", Math.Round(BgmSlider.value * 100));
                    break;
                case ConfigType.Se:
                    LocalDataManager.Instance.Option.SeVolume = SeSlider.value;
                    LocalDataManager.Instance.Option.IsMuteSe = LocalDataManager.Instance.Option.SeVolume == 0;
                    SoundManager.Instance.UpdateSeVolume(LocalDataManager.Instance.Option.SeVolume, LocalDataManager.Instance.Option.IsMuteSe);
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