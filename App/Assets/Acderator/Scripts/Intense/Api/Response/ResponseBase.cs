using MessagePack;
using System.Collections.Generic;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ResponseHeader
    {
        [Key("code")] public int Code { get; set; }
        [Key("message")] public string Message { get; set; }
        [Key("token")] public string Token { get; set; }
        [Key("master")] public Dictionary<string, object> Master { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class ResponseBase
    {
        [Key("header")] public ResponseHeader Header { get; set; }

        [IgnoreMember] public int ErrorCode => Header?.Code ?? -1;
        [IgnoreMember] public string ErrorMessage => Header?.Message ?? string.Empty;
        [IgnoreMember] public Dictionary<string, object> Master => Header?.Master;
        [IgnoreMember] public NetworkError NetworkError => ErrorCode.GetNetworkError();
        [IgnoreMember] public bool IsSuccess => NetworkError == NetworkError.None;
    }
}