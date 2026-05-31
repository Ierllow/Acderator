using Cysharp.Threading.Tasks;
using Intense;
using Zenject;

namespace Song
{
    public sealed class SongSoundController
    {
        [Inject] private readonly SoundManager soundManager;

        private readonly CriAtomPlaybackClock playbackClock = new();

        public void PlaySong(int id) => UniTask.Void(async () =>
        {
            var playback = await soundManager.PlaySongAsync(id);
            playbackClock.Start(playback);
        });

        public void PauseSong(bool isPause) => soundManager.PauseSong(isPause);

        public void StopSong()
        {
            soundManager.StopSong();
            playbackClock.Stop();
        }

        public float GetTimeSec() => playbackClock.GetTimeSec(soundManager.SongExPlayer);

        public bool HasStarted() => GetTimeSec() > 0f;

        public bool IsPlayEnd() => soundManager.SongExPlayer?.IsPlayEnd() ?? true;
    }
}