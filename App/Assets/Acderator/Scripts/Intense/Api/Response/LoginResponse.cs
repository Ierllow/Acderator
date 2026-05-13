using System.Collections.Generic;

namespace Intense.Api
{
    public class LoginResponse : ResponseBase
    {
        public string Token => Header.TryGetString("token", out var token) ? token : string.Empty;

        public LoginResponse(Dictionary<string, object> responseData) : base(responseData) { }
    }
}