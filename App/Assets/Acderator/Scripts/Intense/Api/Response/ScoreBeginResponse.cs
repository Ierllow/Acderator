using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ScoreBeginResponse : ResponseBase
    {
        [Key("session_id")] public string SessionId { get; set; } = string.Empty;
    }
}