#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorHierarchyViewEx
{
    private const int cWidth = 16;

    [InitializeOnLoadMethod]
    private static void HierarchyViewEx()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
    }

    private static void OnGUI(int instanceID, Rect selectionRect)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        var pos = selectionRect;
        pos.x = pos.xMax - cWidth;
        pos.width = cWidth;

        var newActive = GUI.Toggle(pos, obj.activeSelf, string.Empty);

        if (newActive != obj.activeSelf)
        {
            obj.SetActive(newActive);
            EditorUtility.SetDirty(obj);
        }

        pos.x -= cWidth;
        var oldLock = (obj.hideFlags & HideFlags.NotEditable) != 0;
        var newLock = GUI.Toggle(pos, oldLock, string.Empty, "IN LockButton");

        if (newLock != oldLock)
        {
            if (newLock)
            {
                obj.hideFlags |= HideFlags.NotEditable;
            }
            else
            {
                obj.hideFlags &= ~HideFlags.NotEditable;
            }
        }

        pos.x -= cWidth;

        var isWarning = obj.GetComponents<MonoBehaviour>().Any(c => c == null);
        if (isWarning) GUI.Label(pos, "!");
    }
}
#endif