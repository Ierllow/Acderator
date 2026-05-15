using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class LoginResponse : ResponseBase
    {
        [IgnoreMember] public string Token => Header?.Token ?? string.Empty;
    }
}