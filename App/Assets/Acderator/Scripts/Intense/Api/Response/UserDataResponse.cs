using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ScoreEntry
    {
        [Key("score_id")] public int ScoreId { get; set; }
        [Key("score")] public int Score { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class UserDataResponse : ResponseBase
    {
        [Key("scores")] public List<ScoreEntry> Scores { get; set; } = new();

        [IgnoreMember] public IEnumerable<(int sid, int score)> ScorePairs => Scores?.Select(s => (s.ScoreId, s.Score)) ?? Enumerable.Empty<(int, int)>();
    }
}