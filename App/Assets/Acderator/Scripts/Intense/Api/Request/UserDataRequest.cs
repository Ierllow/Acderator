namespace Intense.Api
{
    public class UserDataRequest : RequestBase
    {
        public override string ApiKey => "user/data";

        public override string HttpMethod => UnityEngine.Networking.UnityWebRequest.kHttpVerbGET;
    }
}