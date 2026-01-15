namespace Song
{
    public interface ISongError { }
    public sealed record ScoreLoadError(ELoadResult Result) : ISongError { }
    public sealed record ScoreSaveError() : ISongError { }
    public sealed record AlertError() : ISongError { }
}