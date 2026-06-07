#nullable enable

using Intense;

namespace Song
{
    public sealed class SongPlaybackClock
    {
        private CriAtomPlaybackSession? playbackSession;

        public bool IsPlayEnd => playbackSession?.IsPlayerPlayEnd ?? false;
        public float TimeSec
        {
            get
            {
                if (playbackSession is not { } session) return 0f;

                var syncedTimeSec = session.IsPlaybackRemoved switch
                {
                    true => 0f,
                    _ => session.SyncedTimeSec,
                };
                return syncedTimeSec > 0f ? syncedTimeSec : session.PlayerTimeSec;
            }
        }

        public void Start(CriAtomPlaybackSession playbackSession) => this.playbackSession = playbackSession;

        public void Stop() => playbackSession = null;
    }
}
