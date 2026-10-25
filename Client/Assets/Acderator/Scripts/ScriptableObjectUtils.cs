using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ScriptableObjectUtils
{
#if UNITY_EDITOR
    public static T Load<T>() where T : Object
    {
        var guid = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T))).FirstOrDefault();
        var filePath = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(filePath)
            ? throw new FileNotFoundException(string.Format("{0} does not exists", typeof(T)))
            : AssetDatabase.LoadAssetAtPath<T>(filePath);
    }

    public static void ToAsset<T>(string directoryPath) where T : Object
    {
        var filePath = string.Format("Assets/Acderator/Scripts/{0}/{1}.asset", directoryPath, typeof(T).Name);
        if (!File.Exists(filePath))
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(typeof(T)), filePath);
            AssetDatabase.Refresh();

            Debug.Log(string.Format("{0}.asset is created.", typeof(T).Name));
            return;
        }
        Debug.LogError(string.Format("{0}.asset already exists.", typeof(T).Name));
    }
#endif
}