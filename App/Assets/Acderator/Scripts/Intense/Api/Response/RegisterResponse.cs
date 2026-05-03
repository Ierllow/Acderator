using System.Collections.Generic;

namespace Intense.Api
{
    public class RegisterResponse : ResponseBase
    {
        public string Token => Body.TryGetString("token", out var token) ? token : string.Empty;
        public int UserId => Body.TryGetInt("userid", out var userid) ? userid : 0;
        public string Password => Body.TryGetString("password", out var password) && !string.IsNullOrEmpty(password) ? password : UserId.ToString();

        public RegisterResponse(Dictionary<string, object> responseData) : base(responseData) { }
    }
}