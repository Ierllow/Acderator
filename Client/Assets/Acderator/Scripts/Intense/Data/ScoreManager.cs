using Intense.Master;
using System.Collections.Generic;
using System.Linq;

namespace Intense.Data
{
    internal class ScoreManager
    {
        public List<ScoreData> ScoreDataList { get; } = new();

        public void SetScoreData(IEnumerable<(int sid, int score)> entries)
        {
            foreach (var (sid, score) in entries)
            {
                UpdateScoreData(sid, score);
            }
        }

        public void UpdateScoreData(int sid, int scoreNum)
        {
            var target = ScoreDataList.FirstOrDefault(x => x.Sid == sid);
            if (target != default)
            {
                if (target.ScoreNum >= scoreNum) return;
                ScoreDataList[ScoreDataList.IndexOf(target)] = new() { Sid = sid, ScoreNum = scoreNum };
            }
            else
            {
                ScoreDataList.Add(new() { Sid = sid, ScoreNum = scoreNum });
            }
        }

        public int GetScore(int sid) => ScoreDataList.FirstOrDefault(x => x.Sid == sid)?.ScoreNum ?? 0;
    }
}