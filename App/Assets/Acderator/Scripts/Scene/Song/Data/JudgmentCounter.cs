using Intense;
using System.Collections.Generic;

namespace Song
{
    public class JudgmentCounter
    {
        private readonly Dictionary<EJudgementType, int> judgeCountDict = new()
        {
            { EJudgementType.None, 0 },
            { EJudgementType.Perfect, 0 },
            { EJudgementType.Great, 0 },
            { EJudgementType.Good, 0 },
            { EJudgementType.Bad, 0 },
            { EJudgementType.Miss, 0 },
        };

        public void AddJudgmentCount(EJudgementType judgmentType, int count = 1) => judgeCountDict[judgmentType] += count;
        public Dictionary<EJudgementType, int> GetJudgmentCountDict() => judgeCountDict;
    }
}