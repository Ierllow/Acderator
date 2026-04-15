using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Intense.Attribute
{
    /// <summary> This attribute only supports fields that reference UnityEngine.Object. </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredFieldAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    public sealed class RequiredFieldDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Debug.Assert(property.propertyType.EnumEquals(SerializedPropertyType.ObjectReference));
            var baseHeight = EditorGUI.GetPropertyHeight(property, label, true);
            return IsMissing(property) ? baseHeight + EditorGUIUtility.singleLineHeight * 1.2f : baseHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!property.propertyType.EnumEquals(SerializedPropertyType.ObjectReference))
            {
                EditorGUI.HelpBox(position, string.Format("{0} must be a UnityEngine.Object reference.", label.text), MessageType.Error);
                return;
            }

            var fieldRect = position;
            fieldRect.height = EditorGUI.GetPropertyHeight(property, label, true);

            EditorGUI.PropertyField(fieldRect, property, label, true);

            if (!IsMissing(property)) return;

            var helpRect = position;
            helpRect.y += fieldRect.height + 2f;
            helpRect.height = EditorGUIUtility.singleLineHeight * 1.2f;

            EditorGUI.HelpBox(helpRect, string.Format("{0} is required.", label.text), MessageType.Error);
        }

        private bool IsMissing(SerializedProperty property) => property.objectReferenceValue == null;
    }

    [InitializeOnLoad]
    public static class RequiredFieldValidator
    {
        static RequiredFieldValidator()
        {
            EditorApplication.delayCall += ValidateOpenScenes;
            EditorApplication.hierarchyChanged += ValidateOpenScenes;
        }

        private static void ValidateOpenScenes()
        {
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;

                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var fields = behaviour.GetType().GetFields(flags);

                foreach (var field in fields)
                {
                    if (!System.Attribute.IsDefined(field, typeof(RequiredFieldAttribute))) continue;
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)) continue;
                    if (field.GetValue(behaviour) is UnityEngine.Object) continue;

                    Debug.LogError(string.Format("Missing RequiredField: {0}/{1}/{2}.{3}", behaviour.gameObject.scene.path, behaviour.name, behaviour.GetType().Name, field.Name), behaviour);
                }
            }
        }
    }
}