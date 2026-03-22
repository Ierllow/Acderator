using System.Collections.Generic;

namespace Intense.Api
{
    public class LoginResponse : ResponseBase
    {
        public string Token => responseDate.TryGetValue("token", out var token) ? (string)token : string.Empty;
        public Dictionary<string, object> Master => Header?.TryGetValue("master", out var master) ?? false ? master as Dictionary<string, object> : default;
        public LoginResponse(Dictionary<string, object> responseDate) : base(responseDate) { }
    }
}