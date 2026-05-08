using MessagePack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace Intense.Api
{
    public static class NetworkExtensions
    {
        public static NetworkError GetNetworkError(this int code) => code switch
        {
            10001 => NetworkError.BadRequest,
            10002 => NetworkError.InvalidRequestFormat,
            11001 => NetworkError.Unauthorized,
            11002 => NetworkError.Forbidden,
            12001 => NetworkError.NotFound,
            12002 => NetworkError.Conflict,
            13001 => NetworkError.PreconditionFailed,
            13002 => NetworkError.Maintenance,
            14001 => NetworkError.UnsupportedMediaType,
            14002 => NetworkError.TooManyRequests,
            14003 => NetworkError.PayloadTooLarge,
            15000 => NetworkError.ServiceFailure,
            15001 => NetworkError.ResponseFailure,
            15002 => NetworkError.DataFailure,
            15003 => NetworkError.ResourceFailure,
            _ => default,
        };

        public static bool TryGetInt(this Dictionary<string, object> dictionary, string key, out int value)
        {
            value = 0;
            if (!(dictionary?.TryGetValue(key, out var rawValue) ?? false)) return false;

            switch (rawValue)
            {
                case int intValue:
                    value = intValue;
                    return true;
                case uint uintValue when uintValue <= int.MaxValue:
                    value = (int)uintValue;
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryGetString(this Dictionary<string, object> dictionary, string key, out string value)
        {
            value = string.Empty;
            if (!(dictionary?.TryGetValue(key, out var rawValue) ?? false)) return false;

            value = rawValue.ToString();
            return true;
        }

        public static bool TryGetDictionary(this Dictionary<string, object> dictionary, string key, out Dictionary<string, object> value)
        {
            value = default;
            return (dictionary?.TryGetValue(key, out var rawValue) ?? false) && rawValue.TryConvertDictionary(out value);
        }

        public static bool TryConvertDictionary(this object rawValue, out Dictionary<string, object> value)
        {
            value = default;
            switch (rawValue)
            {
                case Dictionary<string, object> stringDictionary:
                    value = stringDictionary;
                    return true;
                case Dictionary<object, object> objectDictionary:
                    value = objectDictionary.Where(x => !string.IsNullOrEmpty(x.Key?.ToString())).ToDictionary(x => x.Key.ToString(), x => x.Value);
                    return true;
                default:
                    return false;
            }
        }

        public static IEnumerable<object> GetList(this Dictionary<string, object> dictionary, string key) => (dictionary?.TryGetValue(key, out var rawValue) ?? false) && rawValue is IEnumerable<object> enumerable ? enumerable : Enumerable.Empty<object>();

        public static void SetApiRequestHeaders(this UnityWebRequest request, string token, string masterVersion)
        {
            var header = new Dictionary<string, string>
            {
                { "master", masterVersion },
            };
            request.SetRequestHeader("Content-Type", "application/x-msgpack");
            request.SetRequestHeader("Accept", "application/x-msgpack");
            request.SetRequestHeader("header", MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(header)));
            if (!string.IsNullOrEmpty(token)) request.SetRequestHeader("Authorization", string.Format("Bearer {0}", token));
        }

        public static ResponseBase CreateResponse(this RequestBase request, Dictionary<string, object> responseData) => request switch
        {
            RegisterRequest => new RegisterResponse(responseData),
            LoginRequest => new LoginResponse(responseData),
            UserDataRequest => new UserDataResponse(responseData),
            ScoreBeginRequest => new ScoreBeginResponse(responseData),
            ScoreSubmitRequest => new ScoreSubmitResponse(responseData),
            _ => new ResponseBase(responseData),
        };
    }
}
