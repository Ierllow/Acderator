using UnityEditor;
using UnityEngine;

namespace Intense.Api
{
    public class NetworkConfig : ScriptableObject
    {
#if UNITY_EDITOR
        #region Create ScriptableObject
        [MenuItem("Tools/ScriptableObject/Create NetworkConfig")]
        private static void NetworkConfigToAsset() => ScriptableObjectUtils.ToAsset<NetworkConfig>("Intense/Api");

        #endregion
#endif

        public string apiServerUrl = "";
        public string assetServerUrl = "";
        public string webViewServerUrl = "";
    }
}