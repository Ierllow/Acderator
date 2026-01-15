using Cysharp.Text;
using Intense;
using System;
using System.Collections.Generic;

namespace Song
{
    public sealed class SongSceneContext : SceneContext
    {
        public SongInfo SongInfo { get; private init; }
        public SongOption SongOption { get; private init; }
        public ESongMode SongMode { get; private init; }
        public bool IsRestart { get; private set; }
        public TutorialInfo TutorialInfo { get; private init; }
        public TutorialData TutorialData { get; private init; }

        public bool IsAuto => SongOption?.IsAuto ?? false;

        public List<string> SongBundlePathList => new()
        {
            "song/rank",
            "song/difficulty",
            "song/bg",
            "sounds/song/songse",
            "song/particle/judgetext",
            SongBundlePath,
            SongChartBundlePath,
        };
        public string SongChartBundlePath => ZString.Format("charts/{0}", SongInfo.Sid);

        public string SongBundlePath => ZString.Format("sounds/song/song_{0}", SongInfo.Group);

        SongSceneContext() { }

        public static SongSceneContext Create(SongInfo songInfo, bool isAuto, ESongMode songMode, bool isRetry = false) => new()
        {
            SongInfo = songInfo,
            SongOption = new() { IsAuto = isAuto },
            SongMode = songMode,
            IsRestart = isRetry
        };
        public static SongSceneContext Create(SongInfo songInfo, TutorialInfo tutorialInfo = default) => new()
        {
            SongInfo = songInfo,
            SongOption = new() { IsAuto = false },
            SongMode = ESongMode.Tutorial,
            IsRestart = false,
            TutorialInfo = tutorialInfo,
            TutorialData = new()
            {
                TutorialMaster = tutorialInfo.MTutorial,
                StepList = tutorialInfo.MTutorialStepList,
            },
        };
        public void Refresh(bool isRetry = false) => IsRestart = isRetry;
        public Result.ResultSceneContext ToResultSceneContext(int currentScore, Dictionary<EJudgementType, int> judgeCountDict) => new()
        {
            ResultInfo = new()
            {
                Sid = SongInfo.Sid,
                CurrentScore = currentScore,
                JudgeCountDict = judgeCountDict,
                IsAuto = IsAuto,
            }
        };
    }
}