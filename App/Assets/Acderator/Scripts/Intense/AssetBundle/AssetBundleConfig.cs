using UnityEngine;

namespace Intense.Asset
{
    public class AssetBundleConfig : ScriptableObject
    {
        public string manifestFileName = "";
        public string[] assetBundleNameList;
        public int loadCoolDownMs = 500;
    }
}