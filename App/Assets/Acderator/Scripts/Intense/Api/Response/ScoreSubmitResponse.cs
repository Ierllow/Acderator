using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ScoreSubmitResponse : ResponseBase { }
}