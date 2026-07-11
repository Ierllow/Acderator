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
        private readonly HashSet<string> addLoadAddressSet = new();
        private readonly Dictionary<string, AsyncOperationHandle<Object>> loadedAssetHandleDict = new();
        private readonly Dictionary<string, AsyncOperationHandle<IList<Object>>> loadedLabelHandleDict = new();
        private readonly HashSet<SpriteAtlas> spriteAtlasSet = new();

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
            foreach (var handle in loadedAssetHandleDict.Values.Where(handle => handle.IsValid())) Addressables.Release(handle);
            foreach (var handle in loadedLabelHandleDict.Values.Where(handle => handle.IsValid())) Addressables.Release(handle);
            addLoadAddressSet.Clear();
            loadedAssetHandleDict.Clear();
            loadedLabelHandleDict.Clear();
            RefreshSpriteAtlases();
        }

        public Sprite GetSprite(string spriteName) => spriteAtlasSet.FirstOrDefault(sprite => sprite != null).GetSprite(spriteName);

        public void RegisterSpriteAtlas(Object asset)
        {
            if (asset is SpriteAtlas atlas) spriteAtlasSet.Add(atlas);
        }

        private void RefreshSpriteAtlases()
        {
            spriteAtlasSet.Clear();
            spriteAtlasSet.UnionWith(Resources.LoadAll<SpriteAtlas>("Altas"));
            foreach (var handle in loadedAssetHandleDict.Values)
            {
                RegisterSpriteAtlas(handle.Result);
            }
            foreach (var asset in loadedLabelHandleDict.SelectMany(x => x.Value.Result))
            {
                RegisterSpriteAtlas(asset);
            }
        }
    }
}