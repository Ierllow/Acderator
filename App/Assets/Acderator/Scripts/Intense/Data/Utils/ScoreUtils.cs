namespace Intense.Data
{
    public enum RankType { None, New, D, C, B, A, S, Ss, Sss, Exc }

    public static class ScoreUtils
    {
        public static RankType ToRank(int score, bool isShowNew = false) => score switch
        {
            <= 0 when isShowNew => RankType.New,
            <= 0 or < 700000 => RankType.D,
            < 800000 => RankType.C,
            < 850000 => RankType.B,
            < 900000 => RankType.A,
            < 950000 => RankType.S,
            < 980000 => RankType.Ss,
            < 1000000 => RankType.Sss,
            _ => RankType.Exc,
        };
        public static bool IsClear(int score)
        {
            var rank = ToRank(score);
            return rank != RankType.D && rank != RankType.New && rank != RankType.None;
        }
        public static bool IsExc(int score) => ToRank(score) == RankType.Exc;
    }
}