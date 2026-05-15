using MessagePack;
using UnityEngine.Networking;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class UserDataRequest : RequestBase<UserDataResponse>
    {
        [IgnoreMember] public override string ApiKey => "user/data";
        [IgnoreMember] public override string HttpMethod => UnityWebRequest.kHttpVerbGET;
    }
}