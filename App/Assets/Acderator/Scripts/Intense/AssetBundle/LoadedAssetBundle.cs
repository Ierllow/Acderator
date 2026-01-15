using UnityEngine;

namespace Intense.Asset
{
    internal class LoadedAssetBundle
    {
        internal ESceneType SceneType { get; } = SceneManager.Instance.CurrentSceneType;
        internal AssetBundle Bundle { get; }

        internal LoadedAssetBundle(AssetBundle bundle) => Bundle = bundle;
    }
}