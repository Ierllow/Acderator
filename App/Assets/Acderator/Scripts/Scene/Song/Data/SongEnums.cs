namespace Song
{
    public enum ESongMode
    {
        Normal,
        Tutorial,
        TestMode
    }

    public enum ESongState
    {
        None,
        Ready,
        Playing,
        Stop,
        End,
    }

    public enum ESongResultType
    {
        Failed,
        Clear,
        FullCombo,
        Excellent,
    }

    public enum ETutorialState
    {
        None,
        Intro,
        Step,
        Complete
    }

    public enum ELoadResult
    {
        None = 0,
        Unknown,
        Unsupported,
        InvalidAsset,
        Exception,
    }

    public enum ENoteType
    {
        Single,
        Long,
        Flick,
        Curve,
        None = 999,
    }
}