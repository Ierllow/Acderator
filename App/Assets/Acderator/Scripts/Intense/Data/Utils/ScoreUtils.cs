using Intense.Master;
using System;

namespace Intense.Data
{
    public enum ERankType { None, New, D, C, B, A, S, Ss, Sss, Exc }

    public static class ScoreUtils
    {
        public static ERankType ToRank(int score, bool isShowNew = false) => score <= 0
            ? isShowNew ? ERankType.New : ERankType.D
            : score < 700000 ? ERankType.D
            : score < 800000 ? ERankType.C
            : score < 850000 ? ERankType.B
            : score < 900000 ? ERankType.A
            : score < 950000 ? ERankType.S
            : score < 980000 ? ERankType.Ss
            : score < 1000000 ? ERankType.Sss
            : ERankType.Exc;
        public static int ToScorePercent(int sid) => ScoreManager.Instance.GetScore(sid) % MasterDataManager.Instance.MemoryDatabase.SongBaseScoreMasterTable.First().Score / 10000;
        public static bool IsClear(int score) => !ToRank(score).EnumEquals(ERankType.D) || !ToRank(score).EnumEquals(ERankType.New) || !ToRank(score).EnumEquals(ERankType.None);
        public static bool IsExc(int score) => ToRank(score).EnumEquals(ERankType.Exc);
    }
}