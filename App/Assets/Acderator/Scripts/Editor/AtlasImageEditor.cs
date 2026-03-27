using Intense.UI;
using System;
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
    private SerializedProperty atlas;
    private SerializedProperty spriteName;

    private AnimBool showSpriteName;

    private string[] atlasSpriteNames;
    private int spriteNameIndex = 0;

    protected override void OnEnable()
    {
        atlas = serializedObject.FindProperty("m_Atlas");
        spriteName = serializedObject.FindProperty("m_SpriteName");
        showSpriteName = new AnimBool(atlas.objectReferenceValue != null);
        showSpriteName.valueChanged.AddListener(Repaint);

        ResetAtlasSpriteNames();
        ResetSpriteNameIndex();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        showSpriteName.valueChanged.RemoveListener(Repaint);
        base.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        AtlasGUI();

        showSpriteName.target = atlas.objectReferenceValue != null;
        if (EditorGUILayout.BeginFadeGroup(showSpriteName.faded)) SpriteNameGUI();
        EditorGUILayout.EndFadeGroup();

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }

    protected virtual void AtlasGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(atlas);

        if (EditorGUI.EndChangeCheck())
        {
            ResetAtlasSpriteNames();
            ResetSpriteNameIndex();
        }
    }

    private void ResetSpriteNameIndex()
    {
        if (atlasSpriteNames?.Length == 0) return;

        var currentName = spriteName.stringValue;
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
        spriteName.stringValue = atlasSpriteNames[spriteNameIndex];
        UpdateSourceImage();
    }

    private void ResetAtlasSpriteNames()
    {
        var newAtlas = atlas.objectReferenceValue as SpriteAtlas;
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
            spriteName.stringValue = atlasSpriteNames[spriteNameIndex];
            UpdateSourceImage();
        }
    }

    protected virtual void UpdateSourceImage()
    {
        var type = serializedObject.FindProperty("m_Type");
        var sprite = serializedObject.FindProperty("m_Sprite");

        var currentAtlas = atlas.objectReferenceValue as SpriteAtlas;

        if (currentAtlas == null) return;

        var newSprite = currentAtlas.GetSprite(spriteName.stringValue);

        sprite.objectReferenceValue = newSprite;
        if (newSprite)
        {
            var oldType = (Image.Type)type.enumValueIndex;
            if (newSprite.border.SqrMagnitude() > 0)
            {
                type.enumValueIndex = (int)Image.Type.Sliced;
            }
            else if (oldType.EnumEquals(Image.Type.Sliced))
            {
                type.enumValueIndex = (int)Image.Type.Simple;
            }
        }
    }

    private static IEnumerable<Sprite> GetAllSprite(SpriteAtlas spriteAtlas)
    {
        var spriteArray = new Sprite[spriteAtlas.spriteCount];

        spriteAtlas.GetSprites(spriteArray);
        foreach (var sprite in spriteArray)
        {
            yield return sprite;
        }
    }
}