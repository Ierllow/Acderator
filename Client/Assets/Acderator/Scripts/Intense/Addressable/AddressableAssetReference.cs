using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Intense.Asset
{
    [Serializable]
    public sealed class TextAssetReference : AssetReferenceT<TextAsset>
    {
        public TextAssetReference(string guid) : base(guid) { }
    }
}