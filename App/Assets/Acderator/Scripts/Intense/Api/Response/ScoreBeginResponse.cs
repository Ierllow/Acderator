using System.Collections.Generic;

namespace Intense.Api
{
    public class ScoreBeginResponse : ResponseBase
    {
        public string SessionId => responseData.TryGetString("session_id", out var sessionId) ? sessionId : string.Empty;

        public ScoreBeginResponse(Dictionary<string, object> responseData) : base(responseData) { }
    }
}