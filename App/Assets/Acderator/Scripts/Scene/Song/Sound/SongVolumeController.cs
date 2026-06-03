using Intense;

namespace Song
{
    public sealed class SongVolumeController : SoundVolumeControllerBase
    {
        protected override float CurrentVolume => PlayerPrefsValues.SongVolume;
        protected override bool IsMuted => PlayerPrefsValues.IsSongMuted;
    }
}