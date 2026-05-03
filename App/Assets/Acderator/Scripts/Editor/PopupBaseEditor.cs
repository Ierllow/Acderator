using Intense.UI;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(PopupBase), true), CanEditMultipleObjects]
public class PopupBaseEditor : Editor
{
    private const string SetContentSizeMethodName = "SetContentSize";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        if (!GUILayout.Button("Set Content Size")) return;

        foreach (var targetObject in targets)
        {
            var popupBase = targetObject as PopupBase;
            if (popupBase == null) continue;

            Undo.RegisterFullObjectHierarchyUndo(popupBase.gameObject, "Set Content Size");
            InvokeSetContentSize(popupBase);
            EditorUtility.SetDirty(popupBase);
            if (popupBase.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(popupBase.gameObject.scene);
        }
    }

    private static void InvokeSetContentSize(PopupBase popupBase)
    {
        var type = popupBase.GetType();
        while (type != null)
        {
            var method = type.GetMethod(SetContentSizeMethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (method != null)
            {
                method.Invoke(popupBase, null);
                return;
            }

            type = type.BaseType;
        }
    }
}
