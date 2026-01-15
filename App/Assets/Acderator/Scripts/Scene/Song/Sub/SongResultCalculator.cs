using Intense;
using Intense.Data;
using System.Collections.Generic;

namespace Song
{
    public class SongResultCalculator
    {
        private readonly int minClearHpPercent = 70;

        public ESongResultType CalculateResult(float hpPercent, Dictionary<EJudgementType, int> judgeCountDict, int currentCombo, int totalNoteCount)
        {
            if ((int)(hpPercent * HpBarController.MAX_HP_PERCENT) < minClearHpPercent) return ESongResultType.Failed;
            else if (totalNoteCount == judgeCountDict.GetValueOrDefault(EJudgementType.Perfect)) return ESongResultType.Excellent;
            else if (totalNoteCount == currentCombo) return ESongResultType.FullCombo;
            else if (ScoreUtils.IsClear(currentCombo)) return ESongResultType.Clear;
            return ESongResultType.Failed;
        }
    }
}