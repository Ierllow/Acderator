namespace Song
{
    public class SongOption
    {
        public float NoteSpeed { get; } = PlayerPrefsValues.NS;
        public float TapTiming { get; } = PlayerPrefsValues.TN;
        public bool IsAuto { get; init; }
    }
}