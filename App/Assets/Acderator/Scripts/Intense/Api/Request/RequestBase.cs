using System.Collections.Generic;
using PlayFab.ClientModels;

namespace Intense.Api
{
    public abstract class RequestBase
    {
        protected virtual string ApiKey { get; }

        protected virtual Dictionary<string, object> PostData { get; private set; }

        protected virtual UserDataPermission Permission { get; private set; } = UserDataPermission.Private;

        public abstract void AddData(string key, object value);

        public abstract UpdateUserDataRequest CreateUpdateUserDataRequest();
    }
}