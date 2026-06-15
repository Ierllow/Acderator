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
            dictionary.TryGetValue(key, out var rawValue);
            return (value = rawValue switch
            {
                int intValue => intValue,
                uint uintValue when uintValue <= int.MaxValue => (int)uintValue,
                _ => default,
            }) != default;
        }
        public static bool TryGetString(this Dictionary<string, object> dictionary, string key, out string value)
            => (value = dictionary.TryGetValue(key, out var rawValue) ? rawValue.ToString() : string.Empty) != default;

        public static bool TryConvertDictionary(this object rawValue, out Dictionary<string, object> value) => (value = rawValue switch
        {
            Dictionary<string, object> stringDictionary => stringDictionary,
            Dictionary<object, object> objectDictionary => objectDictionary.Where(x => !string.IsNullOrEmpty(x.Key?.ToString())).ToDictionary(x => x.Key.ToString(), x => x.Value),
            _ => default
        }) != default;

        public static IEnumerable<object> GetList(this Dictionary<string, object> dictionary, string key) => (dictionary?.TryGetValue(key, out var rawValue) ?? false) && rawValue is IEnumerable<object> enumerable ? enumerable : Enumerable.Empty<object>();

        public static void SetApiRequestHeaders(this UnityWebRequest request, string token, string masterVersion)
        {
            request.SetRequestHeader("Content-Type", "application/x-msgpack");
            request.SetRequestHeader("Accept", "application/x-msgpack");
            request.SetRequestHeader("header", MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize(new Dictionary<string, string>() { { "master", masterVersion }, })));
            if (!string.IsNullOrEmpty(token)) request.SetRequestHeader("Authorization", string.Format("Bearer {0}", token));
        }
    }
}