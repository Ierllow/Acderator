namespace Intense.Api
{
    public class LoginRequest : RequestBase
    {
        public override string ApiKey { get; } = "auth/login";
    }
}