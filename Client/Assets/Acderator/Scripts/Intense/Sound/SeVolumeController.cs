namespace Intense
{
    public sealed class SeVolumeController : SoundVolumeControllerBase
    {
        protected override float CurrentVolume => PlayerPrefsValues.SeVolume;
        protected override bool IsMuted => PlayerPrefsValues.IsSeMuted;
    }
}