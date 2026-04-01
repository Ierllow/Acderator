#if UNITY_EDITOR
using UnityEngine;

namespace Song
{
    public class DebugConfig : ScriptableObject
    {
        [Header("Debug information reference when isDebug is enable.")]
        [Tooltip("Debug Config")] public bool isDebug = false;
        [Tooltip("Config whether to download the chart for debugging")] public bool isChartDebug = false;
    }
}
#endif