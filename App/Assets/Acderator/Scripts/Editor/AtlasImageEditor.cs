#if UNITY_EDITOR
using Intense.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

[CustomEditor(typeof(AtlasImage), true), CanEditMultipleObjects]
public class AtlasImageEditor : ImageEditor
{
    private SerializedProperty m_Atlas;
    private SerializedProperty m_SpriteName;

    private AnimBool m_ShowSpriteName;

    private string[] atlasSpriteNames;
    private int spriteNameIndex = 0;

    protected override void OnEnable()
    {
        m_Atlas = serializedObject.FindProperty("m_Atlas");
        m_SpriteName = serializedObject.FindProperty("m_SpriteName");
        m_ShowSpriteName = new AnimBool(m_Atlas.objectReferenceValue != null);
        m_ShowSpriteName.valueChanged.AddListener(Repaint);

        ResetAtlasSpriteNames();
        ResetSpriteNameIndex();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        m_ShowSpriteName.valueChanged.RemoveListener(Repaint);
        base.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AtlasGUI();

        m_ShowSpriteName.target = m_Atlas.objectReferenceValue != null;
        if (EditorGUILayout.BeginFadeGroup(m_ShowSpriteName.faded))
        {
            SpriteNameGUI();
        }
        EditorGUILayout.EndFadeGroup();

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }

    protected virtual void AtlasGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Atlas);

        if (EditorGUI.EndChangeCheck())
        {
            ResetAtlasSpriteNames();
            ResetSpriteNameIndex();
        }
    }

    private void ResetSpriteNameIndex()
    {
        if (atlasSpriteNames?.Length == 0)
        {
            return;
        }

        var currentName = m_SpriteName.stringValue;
        var tempIndex = 0;
        for (var i = 0; i < atlasSpriteNames.Length; i++)
        {
            if (currentName == atlasSpriteNames[i])
            {
                tempIndex = i;
                break;
            }
        }
        spriteNameIndex = tempIndex;
        m_SpriteName.stringValue = atlasSpriteNames[spriteNameIndex];
        UpdateSourceImage();
    }

    private void ResetAtlasSpriteNames()
    {
        var newAtlas = m_Atlas.objectReferenceValue as SpriteAtlas;
        if (newAtlas)
        {
            atlasSpriteNames = GetAllSprite(newAtlas).Select(x => x.name.Replace("(Clone)", "")).ToArray();
        }
    }

    protected virtual void SpriteNameGUI()
    {
        EditorGUI.BeginChangeCheck();
        if (atlasSpriteNames != null)
        {
            spriteNameIndex = EditorGUILayout.Popup("SpriteName", spriteNameIndex, atlasSpriteNames);
        }

        if (EditorGUI.EndChangeCheck())
        {
            m_SpriteName.stringValue = atlasSpriteNames[spriteNameIndex];
            UpdateSourceImage();
        }
    }


    protected virtual void UpdateSourceImage()
    {
        var m_Type = serializedObject.FindProperty("m_Type");
        var m_Sprite = serializedObject.FindProperty("m_Sprite");

        var currentAtlas = m_Atlas.objectReferenceValue as SpriteAtlas;

        if (currentAtlas == null)
            return;

        var newSprite = currentAtlas.GetSprite(m_SpriteName.stringValue);

        m_Sprite.objectReferenceValue = newSprite;
        if (newSprite)
        {
            var oldType = (Image.Type)m_Type.enumValueIndex;
            if (newSprite.border.SqrMagnitude() > 0)
            {
                m_Type.enumValueIndex = (int)Image.Type.Sliced;
            }
            else if (oldType == Image.Type.Sliced)
            {
                m_Type.enumValueIndex = (int)Image.Type.Simple;
            }
        }
    }

    static IEnumerable<Sprite> GetAllSprite(SpriteAtlas spriteAtlas)
    {
        var spriteArray = new Sprite[spriteAtlas.spriteCount];

        spriteAtlas.GetSprites(spriteArray);
        foreach (var sprite in spriteArray)
        {
            yield return sprite;
        }
    }
}
#endif