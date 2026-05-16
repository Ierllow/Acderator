using MessagePack;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class RegisterRequest : RequestBase<RegisterResponse>
    {
        [IgnoreMember] public override string ApiKey => "auth/register";

        [Key("uuid")] public string Uuid { get; set; }
    }
}