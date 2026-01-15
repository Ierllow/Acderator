using PlayFab;
using PlayFab.SharedModels;

namespace Intense.Api
{
    public class ResponseBase<T> where T : PlayFabResultCommon
    {
        public virtual T Result { get; private set; }

        public virtual PlayFabError Error { get; private set; }

        public ResponseBase(T result, PlayFabError error)
        {
            Result = result;
            Error = error;
        }
    }
}