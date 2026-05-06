namespace Song
{
    public class SongOption
    {
        public float NoteSpeed { get; } = PlayerPrefsValues.NoteSpeed;
        public float TapTiming { get; } = PlayerPrefsValues.TapTiming;
        public bool IsAuto { get; init; }
    }
}