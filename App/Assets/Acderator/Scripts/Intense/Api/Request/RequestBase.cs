using System.Collections.Generic;

namespace Intense.Api
{
    public abstract class RequestBase
    {
        public abstract string ApiKey { get; }

        public virtual string HttpMethod => UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST;

        public virtual Dictionary<string, object> PostData { get; } = new();
    }
}