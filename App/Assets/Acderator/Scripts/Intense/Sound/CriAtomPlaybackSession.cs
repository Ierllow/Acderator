using CriWare;

namespace Intense
{
    public readonly struct CriAtomPlaybackSession
    {
        private readonly CriAtomExPlayer exPlayer;
        private readonly CriAtomExPlayback playback;

        public bool IsPlayerPlayEnd => exPlayer.IsPlayEnd();
        public float PlayerTimeSec => exPlayer.GetTime().ToSeconds();
        public float SyncedTimeSec => playback.GetTimeSyncedWithAudio().ToSeconds();
        public bool IsPlaybackRemoved => playback.GetStatus() == CriAtomExPlayback.Status.Removed;

        public CriAtomPlaybackSession(CriAtomExPlayer exPlayer, CriAtomExPlayback playback)
        {
            this.exPlayer = exPlayer;
            this.playback = playback;
        }
    }
}