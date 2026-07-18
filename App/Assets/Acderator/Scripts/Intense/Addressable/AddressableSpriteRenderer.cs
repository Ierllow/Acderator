using System;
using UnityEngine;

namespace Intense.Asset
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AddressableSpriteRenderer : MonoBehaviour
    {
        [SerializeField] private string spriteName = "";

        private SpriteRenderer spriteRenderer;

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        internal void Resolve(Func<string, Sprite> spriteResolver)
        {
            if (string.IsNullOrEmpty(spriteName)) return;

            spriteRenderer ??= GetComponent<SpriteRenderer>();
            var resolvedSprite = spriteResolver(spriteName);
            if (resolvedSprite != null) spriteRenderer.sprite = resolvedSprite;
        }
    }
}