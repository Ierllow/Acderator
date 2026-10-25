namespace Intense
{
    public sealed class BgmVolumeController : SoundVolumeControllerBase
    {
        protected override float CurrentVolume => PlayerPrefsValues.BgmVolume;
        protected override bool IsMuted => PlayerPrefsValues.IsBgmMuted;
    }
}