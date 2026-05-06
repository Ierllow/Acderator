namespace Intense.Data
{
    public enum ERankType { None, New, D, C, B, A, S, Ss, Sss, Exc }

    public static class ScoreUtils
    {
        public static ERankType ToRank(int score, bool isShowNew = false) => score switch
        {
            <= 0 when isShowNew => ERankType.New,
            <= 0 => ERankType.D,
            < 700000 => ERankType.D,
            < 800000 => ERankType.C,
            < 850000 => ERankType.B,
            < 900000 => ERankType.A,
            < 950000 => ERankType.S,
            < 980000 => ERankType.Ss,
            < 1000000 => ERankType.Sss,
            _ => ERankType.Exc,
        };
        public static bool IsClear(int score)
        {
            var rank = ToRank(score);
            return rank != ERankType.D && rank != ERankType.New && rank != ERankType.None;
        }
        public static bool IsExc(int score) => ToRank(score) == ERankType.Exc;
    }
}