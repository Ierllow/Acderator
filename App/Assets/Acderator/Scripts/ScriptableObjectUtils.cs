using Cysharp.Text;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZLinq;

public static class ScriptableObjectUtils
{
#if UNITY_EDITOR
    public static T Load<T>() where T : Object
    {
        var guid = AssetDatabase.FindAssets(ZString.Format("t:{0}", typeof(T))).AsValueEnumerable().FirstOrDefault();
        var filePath = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(filePath)
            ? throw new FileNotFoundException(ZString.Format("{0} does not exists", typeof(T)))
            : AssetDatabase.LoadAssetAtPath<T>(filePath);
    }

    public static void ToAsset<T>(string directoryPath) where T : Object
    {
        var filePath = ZString.Format("Assets/Acderator/Scripts/{0}/{1}.asset", directoryPath, typeof(T).Name);
        if (!File.Exists(filePath))
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(typeof(T)), filePath);
            AssetDatabase.Refresh();

            Debug.Log("networkConfig.asset is created.");
            return;
        }
        Debug.LogError(ZString.Format("{0}.asset already exists.", typeof(T).Name));
    }

    public static MenuItem GetScriptableObjectMenuItem<T>() where T : Object => new(ZString.Format("Tools/ScriptableObject/Create {0}", typeof(T).Name));
#endif
}