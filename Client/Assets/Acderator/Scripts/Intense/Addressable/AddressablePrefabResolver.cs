#nullable enable

using UnityEngine;
using Zenject;

namespace Intense.Asset
{
    internal class AddressablePrefabResolver
    {
        [Inject] private readonly AddressableAssetCache addressableAssetCache = default!;

        public T? GetComponent<T>(string prefabName) where T : Component => TryGetComponent(prefabName, out T? component) ? component : LogMissingPrefab<T>(prefabName);

        public T? GetComponentOrFallback<T>(string prefabName, T? fallback) where T : Component => TryGetComponent(prefabName, out T? component) ? component : fallback ?? LogMissingPrefab<T>(prefabName);

        private bool TryGetComponent<T>(string prefabName, out T? component) where T : Component
        {
            foreach (var loadedAsset in addressableAssetCache.LoadedAssets)
            {
                if (loadedAsset is GameObject go && go.name == prefabName && go.TryGetComponent(out T foundComponent))
                {
                    component = foundComponent;
                    return true;
                }
            }

            component = null;
            return false;
        }

        private T? LogMissingPrefab<T>(string prefabName) where T : Component
        {
            Debug.LogError(string.Format("{0} prefab is not loaded from Addressables.", prefabName));
            return null;
        }
    }
}