using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class LoginRequest : RequestBase<LoginResponse>
    {
        [IgnoreMember] public override string ApiKey => "auth/login";

        [Key("userid")] public string UserId { get; set; } = string.Empty;
        [Key("password")] public string Password { get; set; } = string.Empty;
    }
}