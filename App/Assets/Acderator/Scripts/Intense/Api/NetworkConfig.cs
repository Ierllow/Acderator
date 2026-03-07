using Cysharp.Text;
using System.IO;
using UnityEditor;
using UnityEngine;

public class NetworkConfig : ScriptableObject
{
#if UNITY_EDITOR
    #region Create ScriptableObject
    [MenuItem("Tools/ScriptableObject/Create NetworkConfig")]
    private static void DebugConfigToAsset()
    {
        var directoryPath = "Assets/Acderator/Scripts/Intense/Api";
        var filePath = ZString.Format("{0}/networkConfig.asset", directoryPath);
        if (!File.Exists(filePath))
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            AssetDatabase.CreateAsset(CreateInstance<NetworkConfig>(), filePath);
            AssetDatabase.Refresh();

            Debug.Log("networkConfig.asset is created.");
            return;
        }
        Debug.LogError("networkConfig.asset already exists.");
    }
    #endregion
#endif

    public string apiServerUrl = "";
    public string assetServerUrl = "";
    public string webViewServerUrl = "";
}
