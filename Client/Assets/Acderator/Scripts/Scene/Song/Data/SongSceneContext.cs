#nullable enable

using Intense;
using Intense.Asset;
using System.Collections.Generic;

namespace Song
{
    public sealed class SongSceneContext : SceneContext
    {
        public SongInfo SongInfo { get; private init; } = default!;
        public SongOption SongOption { get; private init; } = default!;
        public SongMode SongMode { get; private init; }
        public bool IsRestart { get; private set; }
        public TutorialInfo? TutorialInfo { get; private init; }
        public TutorialData? TutorialData { get; private init; }
        public string SessionId { get; private init; } = default!;
        public override int FrameRate { get; } = 60;
        public override BgmType BgmType { get; } = BgmType.Stop;

        public IReadOnlyList<AddressableAssetAddress> DynamicAssetAddressList => new[]
        {
            SongAddress,
        };

        public AddressableAssetAddress SongAddress => DynamicAssetAddresses.Song(SongInfo.Group);

        SongSceneContext() { }

        public static SongSceneContext Create(SongInfo songInfo, bool isAuto, SongMode songMode, string sessionId, bool isRetry = false) => new()
        {
            SongInfo = songInfo,
            SongOption = new() { IsAuto = isAuto },
            SongMode = songMode,
            IsRestart = isRetry,
            SessionId = sessionId
        };
        public static SongSceneContext Create(SongInfo songInfo, string sessionId, TutorialInfo tutorialInfo) => new()
        {
            SongInfo = songInfo,
            SongOption = new() { IsAuto = false },
            SongMode = SongMode.Tutorial,
            IsRestart = false,
            TutorialInfo = tutorialInfo,
            TutorialData = new()
            {
                TutorialMaster = tutorialInfo.Tutorial,
                StepList = tutorialInfo.TutorialSteps,
            },
            SessionId = sessionId,
        };
        public void Refresh(bool isRetry = false) => IsRestart = isRetry;
        public Result.ResultSceneContext ToResultSceneContext(int currentScore, Dictionary<JudgementType, int> judgeCountDict) => new()
        {
            ResultInfo = new()
            {
                Sid = SongInfo.Sid,
                CurrentScore = currentScore,
                JudgeCountDict = judgeCountDict,
            }
        };
    }
}