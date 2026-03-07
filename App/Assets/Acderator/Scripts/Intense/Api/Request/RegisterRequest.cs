namespace Intense.Api
{
    public class RegisterRequest : RequestBase
    {
        public override string ApiKey { get; } = "auth/register";
    }
}