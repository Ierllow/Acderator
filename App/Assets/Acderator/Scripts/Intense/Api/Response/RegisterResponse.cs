using System.Collections.Generic;

namespace Intense.Api
{
    public class RegisterResponse : ResponseBase
    {
        public string Token => responseDate.TryGetValue("token", out var token) ? (string)token : string.Empty;
        public int UserId => responseDate.TryGetValue("userid", out var userid) ? (int)userid : 0;
        public string PassWord => responseDate.TryGetValue("password", out var password) ? (string)password : string.Empty;

        public RegisterResponse(Dictionary<string, object> responseDate) : base(responseDate) { }
    }
}