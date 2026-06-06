using Cysharp.Threading.Tasks;
using Intense.Api;
using Intense.UI;
using Object = UnityEngine.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using Zenject;

namespace Intense.Asset
{
    internal enum EAddressableOperationResult { Success, Canceled, Failed }

    internal class AddressableAssetManager : MonoBehaviour, IInitializable
    {
        [SerializeField] private AddressableAssetConfig addressableAssetConfigObject;
        [SerializeField] private NetworkConfig networkConfigObject;

        [Inject] private readonly Loading loading;
        [Inject] private readonly AddressableAssetPopupController addressableAssetPopupController;

        private readonly HashSet<string> addLoadAddressSet = new();
        private readonly Dictionary<string, AsyncOperationHandle<Object>> loadedAssetHandleDict = new();
        private readonly Dictionary<string, AsyncOperationHandle<IList<Object>>> loadedLabelHandleDict = new();
        private readonly Dictionary<string, ESceneType> assetSceneTypeDict = new();
        private readonly HashSet<SpriteAtlas> spriteAtlasSet = new();
        private ESceneType activeSceneType;

        internal AddressableAssetConfig Config => addressableAssetConfigObject;

        public void Initialize() => InitializeCore(destroyCancellationToken).Forget(Debug.LogException);

        private async UniTask<EAddressableOperationResult> InitializeCore(CancellationToken cancellationToken)
        {
            var (isCanceled, result) = await InitializeCoreAsync(destroyCancellationToken).AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();
            return isCanceled ? EAddressableOperationResult.Canceled : result;
        }

        private async UniTask<EAddressableOperationResult> InitializeCoreAsync(CancellationToken cancellationToken)
        {
            AddressablesRuntimeProperties.SetPropertyValue(addressableAssetConfigObject.AssetServerUrl, networkConfigObject.assetServerUrl.TrimEnd('/'));
            var initializeHandle = Addressables.InitializeAsync(false);
            EAddressableOperationResult initializeResult;
            try
            {
                var isCanceled = await UniTask.WaitUntil(() => initializeHandle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
                initializeResult = GetOperationResult(initializeHandle, isCanceled);
            }
            finally
            {
                if (initializeHandle.IsValid()) Addressables.Release(initializeHandle);
            }
            if (initializeResult != EAddressableOperationResult.Success) return initializeResult;

            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            List<string> catalogList;
            EAddressableOperationResult checkResult;
            try
            {
                var isCanceled = await UniTask.WaitUntil(() => checkHandle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
                checkResult = GetOperationResult(checkHandle, isCanceled);
                catalogList = checkResult == EAddressableOperationResult.Success ? checkHandle.Result : default;
            }
            finally
            {
                if (checkHandle.IsValid()) Addressables.Release(checkHandle);
            }
            if (checkResult != EAddressableOperationResult.Success) return checkResult;

            if (catalogList.Count > 0)
            {
                var updateHandle = Addressables.UpdateCatalogs(true, catalogList, false);
                EAddressableOperationResult updateResult;
                try
                {
                    var isCanceled = await UniTask.WaitUntil(() => updateHandle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
                    updateResult = GetOperationResult(updateHandle, isCanceled);
                }
                finally
                {
                    if (updateHandle.IsValid()) Addressables.Release(updateHandle);
                }
                if (updateResult != EAddressableOperationResult.Success) return updateResult;
            }
            return EAddressableOperationResult.Success;
        }

        public async UniTask LoadAssetsAsync(ESceneType currentSceneType, CancellationToken cancellationToken)
        {
            activeSceneType = currentSceneType;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    loading.ShowLoading();
                    var result = await TryLoadAssetsAsync(currentSceneType, cancellationToken);
                    if (result == EAddressableOperationResult.Success || result == EAddressableOperationResult.Canceled) return;
                    if (!await addressableAssetPopupController.TryRetryAssetErrorAsync()) return;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    if (!await addressableAssetPopupController.TryRetryAssetErrorAsync()) return;
                }
                finally
                {
                    await UniTask.Delay(addressableAssetConfigObject.LoadCoolDownMilliseconds);
                    loading.ClearProgressBar();
                }
            }
        }

        public UniTask UnloadAssetsAsync(List<ESceneType> sceneTypes)
        {
            var targets = assetSceneTypeDict.Where(kv => sceneTypes.Contains(kv.Value)).Select(kv => kv.Key).ToList();
            foreach (var address in targets)
            {
                addLoadAddressSet.Remove(address);
                assetSceneTypeDict.Remove(address);
                if (loadedAssetHandleDict.Remove(address, out var assetHandle) && assetHandle.IsValid()) Addressables.Release(assetHandle);
                if (loadedLabelHandleDict.Remove(address, out var labelHandle) && labelHandle.IsValid()) Addressables.Release(labelHandle);
            }
            RefreshSpriteAtlases();
            return UniTask.CompletedTask;
        }

        internal UniTask<T> LoadAssetAsync<T>(AddressableAssetAddress address) where T : Object => LoadAssetAsync<T>(address.Value);

        internal UniTask<T> LoadAssetAsync<T>(AssetReferenceT<T> reference) where T : Object => reference == null || !reference.RuntimeKeyIsValid() ? UniTask.FromResult<T>(default) : LoadAssetAsync<T>(reference.RuntimeKey);

        private async UniTask<T> LoadAssetAsync<T>(object key) where T : Object
        {
            var cacheKey = key.ToString();
            if (loadedAssetHandleDict.TryGetValue(cacheKey, out var loadedHandle) && loadedHandle.Result is T loadedAsset) return loadedAsset;

            var loadHandle = Addressables.LoadAssetAsync<Object>(key);
            await UniTask.WaitUntil(() => loadHandle.IsDone);
            if (GetOperationResult(loadHandle, false) != EAddressableOperationResult.Success)
            {
                if (loadHandle.IsValid()) Addressables.Release(loadHandle);
                return default;
            }

            loadedAssetHandleDict[cacheKey] = loadHandle;
            assetSceneTypeDict[cacheKey] = activeSceneType;
            RegisterAsset(loadHandle.Result);
            return loadHandle.Result as T;
        }

        private async UniTask<EAddressableOperationResult> TryLoadAssetsAsync(ESceneType currentSceneType, CancellationToken cancellationToken)
        {
            var targetList = addLoadAddressSet.Where(x => !loadedAssetHandleDict.ContainsKey(x)).ToList();
            var downloadKeyList = GetDownloadKeyList(currentSceneType, targetList);
            var (downloadSizeResult, downloadSize) = await GetDownloadSizeAsync(downloadKeyList, cancellationToken);
            if (downloadSizeResult != EAddressableOperationResult.Success) return downloadSizeResult;
            if (downloadSize > 0 && !await addressableAssetPopupController.TryDownloadConfirmedAsync(downloadSize))
                return EAddressableOperationResult.Canceled;

            var downloadResult = await DownloadDependenciesAsync(downloadKeyList, cancellationToken);
            if (downloadResult != EAddressableOperationResult.Success) return downloadResult;

            var addressResult = await LoadAddressablesAsync(targetList, currentSceneType, cancellationToken);
            return addressResult != EAddressableOperationResult.Success ? addressResult : await LoadLabelAssetsAsync(addressableAssetConfigObject.GetLabels(currentSceneType), currentSceneType, cancellationToken);
        }

        public void AddLoad(AddressableAssetAddress address) => addLoadAddressSet.Add(address.Value);

        private List<object> GetDownloadKeyList(ESceneType sceneType, IEnumerable<string> addressList)
        {
            var keyList = addressList.Cast<object>().ToList();
            foreach (var label in addressableAssetConfigObject.GetLabels(sceneType))
            {
                AddLabelKey(keyList, label);
            }
            return keyList;
        }

        private void AddLabelKey(ICollection<object> keyList, AssetLabelReference label)
        {
            if (label?.RuntimeKeyIsValid() ?? false) keyList.Add(label.RuntimeKey);
        }

        private async UniTask<(EAddressableOperationResult Result, long Size)> GetDownloadSizeAsync(IEnumerable addressList, CancellationToken cancellationToken)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(addressList);
            EAddressableOperationResult result;
            long size;
            try
            {
                var isCanceled = await UniTask.WaitUntil(() => sizeHandle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
                result = GetOperationResult(sizeHandle, isCanceled);
                size = result == EAddressableOperationResult.Success ? sizeHandle.Result : 0;
            }
            finally
            {
                if (sizeHandle.IsValid()) Addressables.Release(sizeHandle);
            }
            return (result, size);
        }

        private async UniTask<EAddressableOperationResult> DownloadDependenciesAsync(IEnumerable addressList, CancellationToken cancellationToken)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(addressList, Addressables.MergeMode.Union, false);
            try
            {
                var isCanceled = await UniTask.WaitUntil(() =>
                {
                    var status = downloadHandle.GetDownloadStatus();
                    loading.SetDownloadFileSize(status.DownloadedBytes, status.TotalBytes);
                    return downloadHandle.IsDone;
                }, cancellationToken: cancellationToken).SuppressCancellationThrow();
                return GetOperationResult(downloadHandle, isCanceled);
            }
            finally
            {
                if (downloadHandle.IsValid()) Addressables.Release(downloadHandle);
            }
        }

        private async UniTask<EAddressableOperationResult> LoadAddressablesAsync(IEnumerable<string> addressList, ESceneType currentSceneType, CancellationToken cancellationToken)
        {
            foreach (var address in addressList)
            {
                var loadHandle = Addressables.LoadAssetAsync<Object>(address);
                var isCanceled = await UniTask.WaitUntil(() => loadHandle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
                var result = GetOperationResult(loadHandle, isCanceled);
                if (result != EAddressableOperationResult.Success)
                {
                    if (loadHandle.IsValid()) Addressables.Release(loadHandle);
                    return result;
                }

                loadedAssetHandleDict[address] = loadHandle;
                assetSceneTypeDict[address] = currentSceneType;
                RegisterAsset(loadHandle.Result);
            }
            return EAddressableOperationResult.Success;
        }

        internal Sprite GetSprite(string spriteName) => spriteAtlasSet.Select(atlas => atlas.GetSprite(spriteName)).FirstOrDefault(sprite => sprite != null);

        private async UniTask<EAddressableOperationResult> LoadLabelAssetsAsync(IEnumerable<AssetLabelReference> labelListESceneType, ESceneType currentSceneType, CancellationToken cancellationToken)
        {
            foreach (var label in labelListESceneType.Where(x => x?.RuntimeKeyIsValid() ?? false))
            {
                var cacheKey = string.Format("label:{0}", label.labelString);
                if (loadedLabelHandleDict.ContainsKey(cacheKey)) continue;

                var locationHandle = Addressables.LoadResourceLocationsAsync(label.RuntimeKey, typeof(Object));
                try
                {
                    var isCanceled = await UniTask.WaitUntil(() => locationHandle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
                    var locationResult = GetOperationResult(locationHandle, isCanceled);
                    if (locationResult != EAddressableOperationResult.Success) return locationResult;
                    if (locationHandle.Result.Count == 0) continue;

                    var loadHandle = Addressables.LoadAssetsAsync<Object>(locationHandle.Result, RegisterAsset);
                    var loadCanceled = await UniTask.WaitUntil(() => loadHandle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
                    var loadResult = GetOperationResult(loadHandle, loadCanceled);
                    if (loadResult != EAddressableOperationResult.Success)
                    {
                        if (loadHandle.IsValid()) Addressables.Release(loadHandle);
                        return loadResult;
                    }

                    loadedLabelHandleDict[cacheKey] = loadHandle;
                    assetSceneTypeDict[cacheKey] = currentSceneType;
                }
                finally
                {
                    if (locationHandle.IsValid()) Addressables.Release(locationHandle);
                }
            }
            return EAddressableOperationResult.Success;
        }

        private void RegisterAsset(Object asset)
        {
            if (asset is SpriteAtlas atlas) spriteAtlasSet.Add(atlas);
        }

        private void RefreshSpriteAtlases()
        {
            spriteAtlasSet.Clear();
            spriteAtlasSet.UnionWith(Resources.LoadAll<SpriteAtlas>("Altas"));
            foreach (var handle in loadedAssetHandleDict.Values)
            {
                RegisterAsset(handle.Result);
            }
            foreach (var asset in loadedLabelHandleDict.SelectMany(x => x.Value.Result))
            {
                RegisterAsset(asset);
            }
        }

        private EAddressableOperationResult GetOperationResult(AsyncOperationHandle operation, bool isCanceled) => (isCanceled, operation.Status) switch
        {
            (true, _) => EAddressableOperationResult.Canceled,
            (_, AsyncOperationStatus.Succeeded) => EAddressableOperationResult.Success,
            _ => LogOperationFailure(operation),
        };

        private EAddressableOperationResult LogOperationFailure(AsyncOperationHandle operation)
        {
            Debug.LogException(operation.OperationException ?? new InvalidOperationException("Addressables operation failed."));
            return EAddressableOperationResult.Failed;
        }
    }
}