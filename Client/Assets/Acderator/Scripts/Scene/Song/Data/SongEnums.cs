#nullable enable

namespace Song
{
    public enum SongMode
    {
        Normal,
        Tutorial,
        TestMode
    }

    public enum SongState
    {
        None,
        Ready,
        Playing,
        Stop,
        End,
    }

    public enum SongResultType
    {
        Failed,
        Clear,
        FullCombo,
        Excellent,
    }

    public enum TutorialState
    {
        None,
        Intro,
        Step,
        Complete
    }

    public enum NoteType
    {
        Single,
        Long,
        Flick,
        Curve,
        None = 999,
    }
}