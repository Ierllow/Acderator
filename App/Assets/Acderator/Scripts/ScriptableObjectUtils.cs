using Cysharp.Text;
using System.Linq;
using UnityEditor;

public static class ScriptableObjectUtils
{
#if UNITY_EDITOR
    public static T Load<T>() where T : UnityEngine.Object
    {
        var guid = AssetDatabase.FindAssets(ZString.Format("t:{0}", typeof(T))).FirstOrDefault();
        var filePath = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(filePath)
            ? throw new System.IO.FileNotFoundException(ZString.Format("{0} does not exists", typeof(T)))
            : AssetDatabase.LoadAssetAtPath<T>(filePath);
    }
#endif
}