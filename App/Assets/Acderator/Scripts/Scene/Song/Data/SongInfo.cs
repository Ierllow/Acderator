using Intense.Master;

namespace Song
{
    public class SongInfo
    {
        private readonly SongMaster mSong;

        public int Sid => mSong?.Sid ?? 0;
        public int Group => mSong?.Group ?? 0;
        public int Difficulty => mSong?.Difficulty ?? 0;
        public string Name => mSong?.Name ?? "";
        public string Composer => mSong?.Composer ?? "";
        public float Start_offset => mSong?.Start_offset ?? 0f;
        public int Bg => mSong?.Bg ?? 0;

        public SongInfo(SongMaster mSong) => this.mSong = mSong;
    }
}