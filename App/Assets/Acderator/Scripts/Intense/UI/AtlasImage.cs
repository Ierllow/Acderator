using Cysharp.Threading.Tasks;
using Intense.Asset;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Zenject;

namespace Intense.UI
{
    public class AtlasImage : Image
    {
        [Inject] private AssetBundleManager assetBundleManager;

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
            if (string.IsNullOrEmpty(atlasName))
            {
                sprite = default;
                return;
            }

            var obj = await assetBundleManager.GetLoadedObjectAsync(atlasName);
            if (obj is SpriteAtlas atlasSprite)
            {
                m_Atlas = atlasSprite;
                m_SpriteName = spriteName;
                sprite = m_Atlas.GetSprite(m_SpriteName);
            }
        });
    }

    public static class AtlasImageExtensions
    {
        public static void SetAtlasFormat<T>(this AtlasImage atlasImage, string format, T arg0, string atlasName = "") => atlasImage.SetAtlas(string.Format(format, arg0), atlasName);
    }
}