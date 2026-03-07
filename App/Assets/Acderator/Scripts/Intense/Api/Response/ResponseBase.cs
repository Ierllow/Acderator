using System.Collections.Generic;

namespace Intense.Api
{
    public class ResponseBase
    {
        protected readonly Dictionary<string, object> responseDate;

        protected Dictionary<string, object> Header => responseDate.TryGetValue("header", out var header) ? header as Dictionary<string, object> : default;

        public int Status => Header?.TryGetValue("status", out var status) ?? false ? (int)status : 0;

        public string ErrorMessage => responseDate.TryGetValue("error", out var error) ? (string)error : string.Empty;

        public ResponseBase(Dictionary<string, object> responseDate)
        {
            this.responseDate = responseDate;
        }
    }
}