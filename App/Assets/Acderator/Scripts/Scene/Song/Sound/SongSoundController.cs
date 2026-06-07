#nullable enable

using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
using Zenject;

namespace Song
{
    public sealed class SongSoundController
    {
        [Inject] private readonly SoundManager soundManager = default!;
        [Inject] private readonly CriAtomCueSheetLoader cueSheetLoader = default!;
        [Inject] private readonly SongSoundSheetNameResolver soundSheetNameResolver = default!;
        [Inject] private readonly SongVolumeController songVolumeController = default!;

        private readonly CriAtomSoundPlayer songExPlayer = new(true);
        private readonly SongPlaybackClock playbackClock = new();

        public float TimeSec => playbackClock.TimeSec;
        public bool HasStarted => TimeSec > 0f;
        public bool IsPlayEnd => playbackClock.IsPlayEnd;

        public void PlaySong(int id) => UniTask.Void(async () =>
        {
            var mSoundCueName = soundSheetNameResolver.Song;
            var sheet = await cueSheetLoader.GetOrAddCueSheetAsync(mSoundCueName.SheetName, DynamicAssetAddresses.Song(id));
            songVolumeController.Register(songExPlayer);
            var playback = songExPlayer.Play(sheet, id.ToString());
            playbackClock.Start(playback);
        });

        public void PauseSong(bool isPause) => songExPlayer.Pause(isPause);

        public void PlaySe(ESeType type) => soundManager.PlaySe(type);

        public void StopSong()
        {
            songExPlayer.Stop();
            songVolumeController.Unregister(songExPlayer);
            playbackClock.Stop();
        }
    }
}
