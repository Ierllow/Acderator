using Intense.Master;
using System.Collections.Generic;
using ZLinq;

namespace Intense.Data
{
    internal class ScoreManager
    {
        public bool IsInit { get; private set; }

        public List<ScoreData> ScoreDataList { get; private set; } = new();

        public void SetScoreData(Dictionary<string, object> dataDict)
        {
            if (IsInit) return;
            foreach (var (key, value) in dataDict)
            {
                var sid = int.Parse(key);
                var score = int.Parse(value.ToString());
                UpdateScoreData(sid, score);
            }
            IsInit = true;
        }

        public void UpdateScoreData(int sid, int scoreNum)
        {
            var target = ScoreDataList.AsValueEnumerable().FirstOrDefault(x => x.Sid == sid);
            if (target != default)
            {
                if (!IsInit) return;
                if (target.ScoreNum >= scoreNum) return;

                target.ScoreNum = scoreNum;
            }
            else
            {
                ScoreDataList.Add(new() { Sid = sid, ScoreNum = scoreNum });
            }
        }

        public int GetScore(int sid) => ScoreDataList.AsValueEnumerable().FirstOrDefault(x => x.Sid == sid)?.ScoreNum ?? 0;
    }
}
