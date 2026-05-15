using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ScoreBeginRequest : RequestBase<ScoreBeginResponse>
    {
        [IgnoreMember] public override string ApiKey => "score/begin";

        [Key("score_id")] public int ScoreId { get; set; }
    }
}