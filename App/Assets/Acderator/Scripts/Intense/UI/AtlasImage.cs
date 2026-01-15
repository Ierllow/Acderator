using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Intense.Asset;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Intense.UI
{
    public class AtlasImage : Image
    {
        [SerializeField] protected SpriteAtlas m_Atlas;
        [SerializeField] protected string m_SpriteName;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_Atlas == null || string.IsNullOrEmpty(m_SpriteName)) return;
            sprite = m_Atlas.GetSprite(m_SpriteName);
        }

        public virtual void SetAtlas(string spriteName, string atlasName = "") => UniTask.Void(async () =>
        {
            if (!string.IsNullOrEmpty(atlasName))
            {
                m_Atlas = await AssetBundleManager.Instance.GetLoadedObjectAsync(atlasName) as SpriteAtlas;
            }
            m_SpriteName = spriteName;
            sprite = m_Atlas != null && !string.IsNullOrEmpty(m_SpriteName) ? m_Atlas.GetSprite(m_SpriteName) : null;
        });

        protected virtual void DestroySprite()
        {
            if (Application.isPlaying) sprite = null;
        }
    }

    public static class AtlasImageExtensions
    {
        public static void SetAtlasFormat<T>(this AtlasImage atlasImage, string format, T arg0, string atlasName = "") => atlasImage.SetAtlas(ZString.Format(format, arg0), atlasName);
    }
}