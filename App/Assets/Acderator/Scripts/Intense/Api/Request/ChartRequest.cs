using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ChartRequest : RequestBase<ChartResponse>
    {
        [IgnoreMember] public override string ApiKey => "song/chart";

        [Key("sid")] public int Sid { get; set; }
    }
}