using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class RegisterResponse : ResponseBase
    {
        [Key("token")] public string Token { get; set; }
        [Key("userid")] public int UserId { get; set; }
        [Key("password")] public string Password { get; set; }
    }
}