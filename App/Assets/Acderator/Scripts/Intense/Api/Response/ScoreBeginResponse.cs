using System.Collections.Generic;

namespace Intense.Api
{
    public class ScoreBeginResponse : ResponseBase
    {
        public string SessionId => responseDate.TryGetValue("session_id", out var sessionId) ? (string)sessionId : string.Empty;

        public ScoreBeginResponse(Dictionary<string, object> responseDate) : base(responseDate) { }
    }
}