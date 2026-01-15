using PlayFab.ClientModels;
using PlayFab.Json;

namespace Intense.Api
{
    public class UpdateScoreRequest : RequestBase
    {
        protected override string ApiKey { get; } = "score";

        public override void AddData(string key, object value)
        {
            PostData.Add(key, value);
        }

        public override UpdateUserDataRequest CreateUpdateUserDataRequest()
        {
            var request = new UpdateUserDataRequest();
            request.Data.Add(ApiKey, PlayFabSimpleJson.SerializeObject(PostData));
            request.Permission = Permission;
            return request;
        }
    }
}