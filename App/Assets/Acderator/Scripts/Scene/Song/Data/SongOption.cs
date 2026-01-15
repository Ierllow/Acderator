using Intense.Data;

namespace Song
{
    [System.Serializable]
    public class SongOption
    {
        public float NoteSpeed { get; } = LocalDataManager.Instance.Option.NoteSpeed;
        public float TapTiming { get; } = LocalDataManager.Instance.Option.TapTimingNum;
        public bool IsAuto { get; init; }
    }
}