using CriWare;

namespace Intense
{
    public sealed class CriAtomPlaybackClock
    {
        private CriAtomExPlayback playback;

        public void Start(CriAtomExPlayback playback) => this.playback = playback;

        public void Stop() => playback = default;

        public float GetTimeSec(CriAtomExPlayer fallbackPlayer)
        {
            if (fallbackPlayer == default) return 0f;

            if (playback is { } currentPlayback && currentPlayback.GetStatus() != CriAtomExPlayback.Status.Removed)
            {
                var syncedTime = currentPlayback.GetTimeSyncedWithAudio();
                if (syncedTime > 0) return syncedTime.ToSeconds();
            }

            return fallbackPlayer.GetTime().ToSeconds();
        }
    }
}