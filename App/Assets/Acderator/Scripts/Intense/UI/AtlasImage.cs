using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

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
            SetSprite(m_Atlas?.GetSprite(spriteName));
        }

        public void SetSprite(Sprite value) => sprite = value;
    }
}