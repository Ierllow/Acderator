using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class LoginRequest : RequestBase<LoginResponse>
    {
        [IgnoreMember] public override string ApiKey => "auth/login";

        [Key("userid")] public string UserId { get; set; }
        [Key("password")] public string Password { get; set; }
    }
}