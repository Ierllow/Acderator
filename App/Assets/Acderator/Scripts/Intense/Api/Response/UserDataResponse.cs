using System.Collections.Generic;

namespace Intense.Api
{
    public class UserDataResponse : ResponseBase
    {
        public Dictionary<string, object> Scores
        {
            get
            {
                if (Body.TryGetDictionary("scores", out var oldFormatScores)) return oldFormatScores;

                var scores = new Dictionary<string, object>();
                foreach (var item in Body.GetList("scores"))
                {
                    if (!item.TryConvertDictionary(out var scoreData)) continue;
                    if (!scoreData.TryGetInt("score_id", out var scoreId)) continue;
                    if (!scoreData.TryGetInt("score", out var score)) continue;
                    scores[scoreId.ToString()] = score;
                }
                return scores;
            }
        }

        public UserDataResponse(Dictionary<string, object> responseData) : base(responseData) { }
    }
}