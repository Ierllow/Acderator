using Intense.UI;
using System;
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
    private const string CloneSuffix = "(Clone)";
    private const string SpriteNameLabel = "SpriteName";

    private SerializedProperty atlas;
    private SerializedProperty spriteName;
    private SerializedProperty sprite;
    private SerializedProperty imageType;

    private AnimBool showSpriteName;
    private string[] spriteNames = Array.Empty<string>();

    private bool HasAtlas => atlas.objectReferenceValue is SpriteAtlas;

    protected override void OnEnable()
    {
        atlas = serializedObject.FindProperty("m_Atlas");
        spriteName = serializedObject.FindProperty("m_SpriteName");
        sprite = serializedObject.FindProperty("m_Sprite");
        imageType = serializedObject.FindProperty("m_Type");

        showSpriteName = new AnimBool(HasAtlas);
        showSpriteName.valueChanged.AddListener(Repaint);

        RefreshSpriteNames();
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

        DrawAtlasField();

        showSpriteName.target = HasAtlas;
        if (EditorGUILayout.BeginFadeGroup(showSpriteName.faded)) DrawSpriteNamePopup();
        EditorGUILayout.EndFadeGroup();

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }

    protected virtual void DrawAtlasField()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(atlas);

        if (EditorGUI.EndChangeCheck())
        {
            RefreshSpriteNames();
            SelectSpriteName(spriteName.stringValue);
        }
    }

    protected virtual void DrawSpriteNamePopup()
    {
        if (spriteNames.Length == 0) return;

        var index = IndexOfSpriteName(spriteName.stringValue);

        EditorGUI.BeginChangeCheck();
        index = EditorGUILayout.Popup(SpriteNameLabel, index, spriteNames);
        if (EditorGUI.EndChangeCheck()) SelectSpriteName(spriteNames[index]);
    }

    private void SelectSpriteName(string name)
    {
        if (spriteNames.Length == 0) return;

        spriteName.stringValue = spriteNames[IndexOfSpriteName(name)];
        UpdateSourceImage();
    }

    private int IndexOfSpriteName(string name) => Mathf.Max(0, Array.IndexOf(spriteNames, name));

    private void RefreshSpriteNames() => spriteNames = atlas.objectReferenceValue is SpriteAtlas spriteAtlas ? GetSpriteNames(spriteAtlas) : Array.Empty<string>();

    protected virtual void UpdateSourceImage()
    {
        if (atlas.objectReferenceValue is not SpriteAtlas spriteAtlas) return;

        var newSprite = spriteAtlas.GetSprite(spriteName.stringValue);
        sprite.objectReferenceValue = newSprite;
        if (newSprite == null) return;

        var hasBorder = newSprite.border.SqrMagnitude() > 0;
        var currentType = (Image.Type)imageType.enumValueIndex;

        imageType.enumValueIndex = (int)((hasBorder, currentType) switch
        {
            (true, _) => Image.Type.Sliced,
            (false, Image.Type.Sliced) => Image.Type.Simple,
            _ => currentType,
        });
    }

    private static string[] GetSpriteNames(SpriteAtlas spriteAtlas)
    {
        var sprites = new Sprite[spriteAtlas.spriteCount];
        spriteAtlas.GetSprites(sprites);
        return sprites.Select(x => x.name.Replace(CloneSuffix, string.Empty)).ToArray();
    }
}