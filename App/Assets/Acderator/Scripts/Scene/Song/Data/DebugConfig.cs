#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Song
{
    [System.Serializable]
    public class DebugConfig : ScriptableObject
    {
        #region Create ScriptableObject
        [MenuItem("Tools/ScriptableObject/Create DebugConfig")]
        private static void DebugConfigToAsset() => ScriptableObjectUtils.ToAsset<DebugConfig>("Scene/Song/Data");
        #endregion

        [Header("Debug information reference when isDebug is enable.")]
        [Tooltip("Debug Config")] public bool isDebug = false;
        [Tooltip("Config whether to download the chart for debugging")] public bool isChartDebug = false;
    }
}
#endif