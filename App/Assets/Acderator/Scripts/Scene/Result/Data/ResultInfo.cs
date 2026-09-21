using Intense;
using System.Collections.Generic;

namespace Result
{
    public class ResultInfo
    {
        public int Sid { get; init; }
        public float HighScore { get; init; }
        public int CurrentScore { get; init; }
        public Dictionary<JudgementType, int> JudgeCountDict { get; init; }
        public bool IsAuto { get; init; }
    }
}