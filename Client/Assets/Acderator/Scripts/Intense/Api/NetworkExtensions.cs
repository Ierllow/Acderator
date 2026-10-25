using System.Collections.Generic;
using System.Linq;

namespace Intense.Api
{
    public static class NetworkExtensions
    {
        public static bool TryGetInt(this Dictionary<string, object> dictionary, string key, out int value)
        {
            dictionary.TryGetValue(key, out var rawValue);
            return (value = rawValue switch
            {
                int intValue => intValue,
                uint uintValue when uintValue <= int.MaxValue => (int)uintValue,
                long longValue when longValue is <= int.MaxValue and >= int.MinValue => (int)longValue,
                _ => default,
            }) != default;
        }

        public static bool TryGetString(this Dictionary<string, object> dictionary, string key, out string value)
            => (value = dictionary.TryGetValue(key, out var rawValue) ? rawValue.ToString() : string.Empty) != default;

        public static bool TryConvertDictionary(this object rawValue, out Dictionary<string, object> value) => (value = rawValue switch
        {
            Dictionary<string, object> stringDictionary => stringDictionary,
            Dictionary<object, object> objectDictionary => objectDictionary.Where(x => !string.IsNullOrEmpty(x.Key?.ToString())).ToDictionary(x => x.Key.ToString(), x => x.Value),
            _ => default,
        }) != default;

        public static IEnumerable<object> GetList(this Dictionary<string, object> dictionary, string key) =>
            (dictionary?.TryGetValue(key, out var rawValue) ?? false) && rawValue is IEnumerable<object> enumerable ? enumerable : Enumerable.Empty<object>();
    }
}