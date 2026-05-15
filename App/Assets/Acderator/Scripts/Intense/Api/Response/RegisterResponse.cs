using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class RegisterResponse : ResponseBase
    {
        [Key("token")] public string Token { get; set; } = string.Empty;
        [Key("userid")] public int UserId { get; set; }
        [Key("password")] public string RawPassword { get; set; } = string.Empty;

        [IgnoreMember] public string Password => string.IsNullOrEmpty(RawPassword) ? UserId.ToString() : RawPassword;
    }
}