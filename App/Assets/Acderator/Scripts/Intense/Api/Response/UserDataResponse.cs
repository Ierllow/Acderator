using System.Collections.Generic;
using UnityEngine;

namespace Intense.Api
{

    public class UserDataResponse : ResponseBase
    {
        public Dictionary<string, object> Scores => responseDate.TryGetValue("scores", out var userData) ? (Dictionary<string, object>)userData : default;

        public UserDataResponse(Dictionary<string, object> responseDate) : base(responseDate) { }
    }
}