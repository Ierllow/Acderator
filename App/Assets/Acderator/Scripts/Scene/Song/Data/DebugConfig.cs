#if UNITY_EDITOR
using Cysharp.Text;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Song
{
    [System.Serializable]
    public class DebugConfig : ScriptableObject
    {
        #region Create ScriptableObject
        [MenuItem("Tools/ScriptableObject/Create DebugConfig")]
        private static void DebugConfigToAsset()
        {
            var directoryPath = "Assets/Acderator/Scripts/Scenes/Song/Data";
            var filePath = ZString.Format("{0}/debugConfig.asset", directoryPath);
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                AssetDatabase.CreateAsset(CreateInstance<DebugConfig>(), filePath);
                AssetDatabase.Refresh();

                Debug.Log("debugConfig.asset is created.");
                return;
            }
            Debug.LogError("DebugConfig.asset already exists.");
        }
        #endregion

        [Header("Debug information reference when isDebug is enable.")]
        [Tooltip("Debug Config")] public bool isDebug = false;
        [Tooltip("Config whether to download the chart for debugging")] public bool isChartDebug = false;
    }
}
#endif