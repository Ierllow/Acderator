using UnityEngine;

namespace Intense.Asset
{
    internal class LoadedAssetBundle
    {
        internal ESceneType SceneType { get; init; }
        internal AssetBundle Bundle { get; init; }
    }
}