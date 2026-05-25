namespace Song
{
    public record SongOption
    {
        public float NoteSpeed { get; init; } = PlayerPrefsValues.NoteSpeed;
        public float TapTiming { get; init; } = PlayerPrefsValues.TapTiming;
        public bool IsAuto { get; init; }
    }
}