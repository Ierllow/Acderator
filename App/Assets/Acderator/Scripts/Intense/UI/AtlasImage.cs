using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using System;

namespace Intense.UI
{
    public class AtlasImage : Image
    {
        [SerializeField] protected SpriteAtlas m_Atlas;
        [SerializeField] protected string m_SpriteName;

        protected override void Awake()
        {
            base.Awake();
            if (m_Atlas == null || string.IsNullOrEmpty(m_SpriteName)) return;
            SetAtlas(m_SpriteName);
        }

        public virtual void SetAtlas(string spriteName)
        {
            m_SpriteName = spriteName;
            if (m_Atlas != null) SetSprite(m_Atlas.GetSprite(spriteName));
        }

        public void SetSprite(Sprite value) => sprite = value;

        public void ResolveSprite(Func<string, Sprite> spriteResolver)
        {
            if (spriteResolver == null) return;
            if (string.IsNullOrEmpty(m_SpriteName)) return;

            var resolvedSprite = spriteResolver(m_SpriteName);
            if (resolvedSprite != null) SetSprite(resolvedSprite);
        }
    }
}