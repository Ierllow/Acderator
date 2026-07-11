using System;

namespace Intense.Asset
{
    public readonly struct AddressableAssetAddress : IEquatable<AddressableAssetAddress>
    {
        public string Value { get; }

        public AddressableAssetAddress(string value) => Value = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Address must not be empty.", nameof(value)) : value;

        public bool Equals(AddressableAssetAddress other) => Value == other.Value;
    }

    public static class DynamicAssetAddresses
    {
        public static AddressableAssetAddress Song(int group) => new(string.Format("sounds/song/song_{0}", group));

        public static AddressableAssetAddress SongChart(int sid) => new(string.Format("charts/{0}", sid));
    }
}