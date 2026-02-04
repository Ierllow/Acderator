using Intense.Data;

namespace Song
{
    public class SongOption
    {
        public float NoteSpeed { get; } = LocalDataManager.Instance.Option.NoteSpeed;
        public float TapTiming { get; } = LocalDataManager.Instance.Option.TapTimingNum;
        public bool IsAuto { get; init; }
    }
}