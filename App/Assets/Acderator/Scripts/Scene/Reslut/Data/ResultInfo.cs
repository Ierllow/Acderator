using Intense;
using System.Collections.Generic;

namespace Result
{
    [System.Serializable]
    public class ResultInfo
    {
        public int Sid { get; init; }
        public float HighScore { get; init; }
        public int CurrentScore { get; init; }
        public Dictionary<EJudgementType, int> JudgeCountDict { get; init; }
        public bool IsAuto { get; init; }
    }
}