using System.Collections.Generic;

namespace Intense.Api
{
    public abstract class RequestBase
    {
        public abstract string ApiKey { get; }

        public virtual Dictionary<string, object> PostData { get; } = new();
    }
}