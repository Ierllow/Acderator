using CriWare;

namespace Intense
{
    public readonly struct CriAtomPlaybackSession
    {
        private readonly CriAtomExPlayer exPlayer;
        private readonly CriAtomExPlayback playback;

        public bool IsPlayerPlayEnd => exPlayer.GetStatus() == CriAtomExPlayer.Status.PlayEnd;
        public float PlayerTimeSec => exPlayer.GetTime() / 1000f;
        public float SyncedTimeSec => playback.GetTimeSyncedWithAudio() / 1000f;
        public bool IsPlaybackRemoved => playback.GetStatus() == CriAtomExPlayback.Status.Removed;

        public CriAtomPlaybackSession(CriAtomExPlayer exPlayer, CriAtomExPlayback playback)
        {
            this.exPlayer = exPlayer;
            this.playback = playback;
        }
    }
}