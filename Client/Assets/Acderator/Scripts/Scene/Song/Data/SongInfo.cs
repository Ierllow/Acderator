#nullable enable

using Intense.Master;

namespace Song
{
    public record SongInfo
    {
        private readonly SongMaster song;

        public int Sid => song?.Sid ?? 0;
        public int Group => song?.Group ?? 0;
        public int Difficulty => song?.Difficulty ?? 0;
        public string Name => song?.Name ?? "";
        public string Composer => song?.Composer ?? "";
        public int Bg => song?.Bg ?? 0;

        public SongInfo(SongMaster song) => this.song = song;
    }
}