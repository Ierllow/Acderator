using System;
using UnityEngine;
using ZLinq;

namespace Intense.Asset
{
    internal class AssetBundleManifestInfo
    {
        public string BundleName { get; }
        public Hash128 Hash { get; }
        public uint Crc { get; }
        public int FileSize { get; }

        public AssetBundleManifestInfo(string rawText)
        {
            var column = rawText.Split(new string[] { "," }, StringSplitOptions.None);
            BundleName = column.AsValueEnumerable().ElementAtOrDefault(0);
            Hash = Hash128.Parse(column.AsValueEnumerable().ElementAtOrDefault(1));
            if (uint.TryParse(column.AsValueEnumerable().ElementAtOrDefault(2), out var crc)) Crc = crc;
            if (int.TryParse(column.AsValueEnumerable().ElementAtOrDefault(3), out var fileSize)) FileSize = fileSize;
        }
    }
}