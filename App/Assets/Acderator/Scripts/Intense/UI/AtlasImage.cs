using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Intense.UI
{
    public class AtlasImage : Image
    {
        [FormerlySerializedAs("m_Atlas"), SerializeField] protected SpriteAtlas atlas;
        [FormerlySerializedAs("m_SpriteName"), SerializeField] protected string spriteName;

        protected override void Awake()
        {
            base.Awake();
            if (atlas == null || string.IsNullOrEmpty(spriteName)) return;
            SetAtlas(spriteName);
        }

        public virtual void SetAtlas(string spriteName)
        {
            this.spriteName = spriteName;
            if (atlas != null) SetSprite(atlas.GetSprite(spriteName));
        }

        public void SetSprite(Sprite value) => sprite = value;

        public void ResolveSprite(Func<string, Sprite> spriteResolver)
        {
            if (spriteResolver == null) return;
            if (string.IsNullOrEmpty(spriteName)) return;

            var resolvedSprite = spriteResolver(spriteName);
            if (resolvedSprite != null) SetSprite(resolvedSprite);
        }
    }
}