using System.Collections.Generic;

namespace Intense.Api
{
    public class ResponseBase
    {
        protected readonly Dictionary<string, object> responseData;

        protected Dictionary<string, object> Header => responseData.TryGetDictionary("header", out var header) ? header : default;
        protected Dictionary<string, object> Body => responseData.TryGetDictionary("body", out var body) ? body : default;
        public int ErrorCode => Header.TryGetInt("code", out var code) ? code : -1;
        public string ErrorMessage => Header.TryGetString("message", out var message) ? message : string.Empty;
        public Dictionary<string, object> Master => Header.TryGetDictionary("master", out var master) ? master : default;

        public NetworkError NetworkError => ErrorCode.GetNetworkError();
        public bool IsSuccess => NetworkError == NetworkError.None;

        public ResponseBase(Dictionary<string, object> responseData) => this.responseData = responseData;
    }
}