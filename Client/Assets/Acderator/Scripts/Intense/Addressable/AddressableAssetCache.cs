using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace Intense.Asset
{
    internal class AddressableAssetCache
    {
        private const string ResourcesAtlasPath = "Altas";

        private readonly HashSet<string> addLoadAddressSet = new();
        private readonly Dictionary<string, AsyncOperationHandle<Object>> loadedAssetHandleDict = new();
        private readonly Dictionary<string, AsyncOperationHandle<IList<Object>>> loadedLabelHandleDict = new();
        private readonly HashSet<SpriteAtlas> spriteAtlasSet = new();
        private readonly Dictionary<string, Sprite> spriteDict = new();

        public IEnumerable<Object> LoadedAssets
        {
            get
            {
                var loadedAssets = loadedAssetHandleDict.Values.Select(handle => handle.Result);
                var loadedLabelAssets = loadedLabelHandleDict.Values.SelectMany(handle => handle.Result);
                return loadedAssets.Concat(loadedLabelAssets);
            }
        }

        public void AddLoad(string address) => addLoadAddressSet.Add(address);

        public List<string> GetUnloadedAddresses() => addLoadAddressSet.Where(x => !loadedAssetHandleDict.ContainsKey(x)).ToList();

        public bool IsLabelLoaded(string cacheKey) => loadedLabelHandleDict.ContainsKey(cacheKey);

        public bool TryGetAsset<T>(string cacheKey, out T asset) where T : Object
        {
            if (loadedAssetHandleDict.TryGetValue(cacheKey, out var handle) && handle.Result is T loaded)
            {
                asset = loaded;
                return true;
            }
            asset = default;
            return false;
        }

        public void CacheAsset(string cacheKey, AsyncOperationHandle<Object> handle)
        {
            loadedAssetHandleDict[cacheKey] = handle;
            RegisterSpriteAtlas(handle.Result);
        }

        public void CacheLabel(string cacheKey, AsyncOperationHandle<IList<Object>> handle) => loadedLabelHandleDict[cacheKey] = handle;

        public void UnloadAll()
        {
            foreach (var handle in loadedAssetHandleDict.Values) ReleaseIfValid(handle);
            foreach (var handle in loadedLabelHandleDict.Values) ReleaseIfValid(handle);
            addLoadAddressSet.Clear();
            loadedAssetHandleDict.Clear();
            loadedLabelHandleDict.Clear();
            spriteDict.Clear();
            spriteAtlasSet.Clear();
            spriteAtlasSet.UnionWith(Resources.LoadAll<SpriteAtlas>(ResourcesAtlasPath));
        }

        public Sprite GetSprite(string spriteName)
        {
            if (spriteDict.TryGetValue(spriteName, out var cached)) return cached;

            foreach (var atlas in spriteAtlasSet)
            {
                if (atlas == null) continue;

                var sprite = atlas.GetSprite(spriteName);
                if (sprite != null)
                {
                    spriteDict[spriteName] = sprite;
                    return sprite;
                }
            }
            return null;
        }

        public void RegisterSpriteAtlas(Object asset)
        {
            if (asset is SpriteAtlas atlas) spriteAtlasSet.Add(atlas);
        }

        private void ReleaseIfValid(AsyncOperationHandle handle)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
    }
}