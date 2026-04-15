using System;
using System.Linq;
using UnityEngine;

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
            BundleName = column.ElementAtOrDefault(0);
            Hash = Hash128.Parse(column.ElementAtOrDefault(1));
            if (uint.TryParse(column.ElementAtOrDefault(2), out var crc)) Crc = crc;
            if (int.TryParse(column.ElementAtOrDefault(3), out var fileSize)) FileSize = fileSize;
        }
    }
}