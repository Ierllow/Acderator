using Intense;

namespace Song
{
    public sealed class SongPlaybackClock
    {
        private CriAtomPlaybackSession playbackSession;

        public bool IsPlayEnd => playbackSession.IsPlayerPlayEnd;
        public float TimeSec
        {
            get
            {
                var syncedTimeSec = playbackSession.IsPlaybackRemoved switch
                {
                    true => 0f,
                    _ => playbackSession.SyncedTimeSec,
                };
                return syncedTimeSec > 0f ? syncedTimeSec : playbackSession.PlayerTimeSec;
            }
        }

        public void Start(CriAtomPlaybackSession playbackSession) => this.playbackSession = playbackSession;

        public void Stop() => playbackSession = default;
    }
}