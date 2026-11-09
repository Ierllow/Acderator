using Intense.Master;
using System.Collections.Generic;

namespace Intense.Data
{
    internal class ScoreManager
    {
        public List<ScoreData> ScoreDataList { get; } = new();

        private readonly Dictionary<int, int> scoreDict = new();

        public void SetScoreData(IEnumerable<(int sid, int score)> entries)
        {
            foreach (var (sid, score) in entries)
            {
                UpdateScoreData(sid, score);
            }
        }

        public void UpdateScoreData(int sid, int scoreNum)
        {
            if (scoreDict.TryGetValue(sid, out var currentScore))
            {
                if (currentScore >= scoreNum) return;
                scoreDict[sid] = scoreNum;
                var index = ScoreDataList.FindIndex(x => x.Sid == sid);
                ScoreDataList[index] = new() { Sid = sid, ScoreNum = scoreNum };
            }
            else
            {
                scoreDict[sid] = scoreNum;
                ScoreDataList.Add(new() { Sid = sid, ScoreNum = scoreNum });
            }
        }

        public int GetScore(int sid) => scoreDict.TryGetValue(sid, out var score) ? score : 0;
    }
}