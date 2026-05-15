using MessagePack;
using UnityEngine.Networking;

namespace Intense.Api
{
    public abstract class RequestBase
    {
        [IgnoreMember] public abstract string ApiKey { get; }
        [IgnoreMember] public virtual string HttpMethod => UnityWebRequest.kHttpVerbPOST;
        [IgnoreMember] public abstract object RequestObject { get; }

        public abstract ResponseBase DeserializeResponse(byte[] bytes);
    }

    public abstract class RequestBase<TResponse> : RequestBase where TResponse : ResponseBase, new()
    {
        [IgnoreMember] public override object RequestObject => this;

        public override ResponseBase DeserializeResponse(byte[] bytes) => MessagePackSerializer.Deserialize<TResponse>(bytes);
    }
}