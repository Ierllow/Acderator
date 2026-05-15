using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ScoreSubmitRequest : RequestBase<ScoreSubmitResponse>
    {
        [IgnoreMember] public override string ApiKey => "score/submit";

        [Key("session_id")] public string SessionId { get; set; } = string.Empty;
        [Key("score")] public int Score { get; set; }
    }
}