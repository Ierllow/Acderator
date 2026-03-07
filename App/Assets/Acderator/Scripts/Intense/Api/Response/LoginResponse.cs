using System.Collections.Generic;

namespace Intense.Api
{
    public class LoginResponse : ResponseBase
    {
        public string Token => responseDate.TryGetValue("token", out var token) ? (string)token : string.Empty;

        public LoginResponse(Dictionary<string, object> responseDate) : base(responseDate) { }
    }
}