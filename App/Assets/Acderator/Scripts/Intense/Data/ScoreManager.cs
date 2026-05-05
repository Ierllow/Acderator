using Intense.Master;
using System.Linq;
using System.Collections.Generic;

namespace Intense.Data
{
    internal class ScoreManager
    {
        public bool IsInit { get; private set; }

        public List<ScoreData> ScoreDataList { get; } = new();

        public void SetScoreData(Dictionary<string, object> dataDict)
        {
            foreach (var (key, value) in dataDict)
            {
                var sid = int.Parse(key);
                var score = int.Parse(value.ToString());
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
