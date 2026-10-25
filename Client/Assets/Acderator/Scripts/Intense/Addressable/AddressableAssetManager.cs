using Cysharp.Threading.Tasks;
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
using Zenject;

namespace Intense.Asset
{
    internal enum AddressableOperationResult { Success, Canceled, Failed }

    internal class AddressableAssetManager : MonoBehaviour, IInitializable, ISceneUnloadHandler
    {
        [SerializeField] private AddressableAssetConfig addressableAssetConfigObject;

        [Inject] private readonly Loading loading;
        [Inject] private readonly AddressableAssetPopupController addressableAssetPopupController;
        [Inject] private readonly AddressableAssetCache addressableAssetCache;

        private UniTask<AddressableOperationResult> initializeTask;

        internal AddressableAssetConfig Config => addressableAssetConfigObject;

        public void Initialize() => initializeTask = InitializeCoreAsync(destroyCancellationToken).Preserve();

        private async UniTask<AddressableOperationResult> InitializeCoreAsync(CancellationToken cancellationToken)
        {
            AddressablesRuntimeProperties.SetPropertyValue(addressableAssetConfigObject.AssetServerUrlPropertyName, addressableAssetConfigObject.AssetServerUrl.TrimEnd('/'));

            var (initializeResult, _) = await AwaitHandleAsync(Addressables.InitializeAsync(false), cancellationToken);
            if (initializeResult != AddressableOperationResult.Success) return LogInitializeResult(initializeResult);

            var (checkResult, catalogList) = await AwaitHandleAsync(Addressables.CheckForCatalogUpdates(false), cancellationToken);
            if (checkResult != AddressableOperationResult.Success || catalogList.Count == 0) return LogInitializeResult(checkResult);

            var (updateResult, _) = await AwaitHandleAsync(Addressables.UpdateCatalogs(true, catalogList, false), cancellationToken);
            return LogInitializeResult(updateResult);
        }

        private AddressableOperationResult LogInitializeResult(AddressableOperationResult result)
        {
            if (result == AddressableOperationResult.Failed) Debug.LogError("Addressables initialization failed.");
            return result;
        }

        public async UniTask LoadAssetsAsync(SceneType currentSceneType, CancellationToken cancellationToken)
        {
            await initializeTask;

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = AddressableOperationResult.Failed;
                loading.ShowLoading();
                try
                {
                    result = await TryLoadAssetsAsync(currentSceneType, cancellationToken);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    loading.ClearProgressBar();
                }

                switch (result)
                {
                    case AddressableOperationResult.Success:
                        ResolveSceneSprites();
                        return;
                    case AddressableOperationResult.Canceled:
                        return;
                }

                if (!await addressableAssetPopupController.TryRetryAssetErrorAsync()) return;
                await UniTask.Delay(addressableAssetConfigObject.LoadCoolDownMilliseconds, cancellationToken: cancellationToken).SuppressCancellationThrow();
            }
        }

        public UniTask OnSceneUnloadingAsync()
        {
            addressableAssetCache.UnloadAll();
            return UniTask.CompletedTask;
        }

        public void AddLoad(AddressableAssetAddress address) => addressableAssetCache.AddLoad(address.Value);

        internal Sprite GetSprite(string spriteName) => addressableAssetCache.GetSprite(spriteName);

        internal UniTask<T> LoadAssetAsync<T>(AddressableAssetAddress address) where T : Object => LoadAssetAsync<T>(address.Value);

        internal UniTask<T> LoadAssetAsync<T>(AssetReferenceT<T> reference) where T : Object => reference == null || !reference.RuntimeKeyIsValid() ? UniTask.FromResult<T>(default) : LoadAssetAsync<T>(reference.RuntimeKey);

        private async UniTask<T> LoadAssetAsync<T>(object key) where T : Object
        {
            await initializeTask;

            var cacheKey = key.ToString();
            if (addressableAssetCache.TryGetAsset<T>(cacheKey, out var cachedAsset)) return cachedAsset;

            var loadHandle = Addressables.LoadAssetAsync<Object>(key);
            if (await WaitForCompletionAsync(loadHandle, CancellationToken.None) != AddressableOperationResult.Success)
            {
                ReleaseIfValid(loadHandle);
                return default;
            }

            addressableAssetCache.CacheAsset(cacheKey, loadHandle);
            return loadHandle.Result as T;
        }

        private async UniTask<AddressableOperationResult> TryLoadAssetsAsync(SceneType currentSceneType, CancellationToken cancellationToken)
        {
            var addressList = addressableAssetCache.GetUnloadedAddresses();
            var labels = addressableAssetConfigObject.GetLabels(currentSceneType).Where(IsValidLabel).ToList();
            var downloadKeyList = addressList.Cast<object>().Concat(labels.Select(label => label.RuntimeKey)).ToList();

            var (sizeResult, downloadSize) = await AwaitHandleAsync(Addressables.GetDownloadSizeAsync((IEnumerable)downloadKeyList), cancellationToken);
            if (sizeResult != AddressableOperationResult.Success) return sizeResult;
            if (downloadSize > 0 && !await addressableAssetPopupController.TryDownloadConfirmedAsync(downloadSize)) return AddressableOperationResult.Canceled;

            var downloadResult = await DownloadDependenciesAsync(downloadKeyList, cancellationToken);
            if (downloadResult != AddressableOperationResult.Success) return downloadResult;

            var addressResult = await LoadAddressesAsync(addressList, cancellationToken);
            return addressResult != AddressableOperationResult.Success ? addressResult : await LoadLabelsAsync(labels, cancellationToken);
        }

        private async UniTask<AddressableOperationResult> DownloadDependenciesAsync(IEnumerable keyList, CancellationToken cancellationToken)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(keyList, Addressables.MergeMode.Union, false);
            var isCanceled = await UniTask.WaitUntil(() =>
            {
                var status = downloadHandle.GetDownloadStatus();
                loading.SetDownloadFileSize(status.DownloadedBytes, status.TotalBytes);
                return downloadHandle.IsDone;
            }, cancellationToken: cancellationToken).SuppressCancellationThrow();
            var result = GetOperationResult(downloadHandle, isCanceled);
            ReleaseIfValid(downloadHandle);
            return result;
        }

        private async UniTask<AddressableOperationResult> LoadAddressesAsync(IEnumerable<string> addressList, CancellationToken cancellationToken)
        {
            foreach (var address in addressList)
            {
                var loadHandle = Addressables.LoadAssetAsync<Object>(address);
                var result = await WaitForCompletionAsync(loadHandle, cancellationToken);
                if (result != AddressableOperationResult.Success)
                {
                    ReleaseIfValid(loadHandle);
                    return result;
                }

                addressableAssetCache.CacheAsset(address, loadHandle);
            }
            return AddressableOperationResult.Success;
        }

        private async UniTask<AddressableOperationResult> LoadLabelsAsync(IEnumerable<AssetLabelReference> labels, CancellationToken cancellationToken)
        {
            foreach (var label in labels)
            {
                var result = await LoadLabelAsync(label, cancellationToken);
                if (result != AddressableOperationResult.Success) return result;
            }
            return AddressableOperationResult.Success;
        }

        private async UniTask<AddressableOperationResult> LoadLabelAsync(AssetLabelReference label, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format("label:{0}", label.labelString);
            if (addressableAssetCache.IsLabelLoaded(cacheKey)) return AddressableOperationResult.Success;

            var locationHandle = Addressables.LoadResourceLocationsAsync(label.RuntimeKey, typeof(Object));
            try
            {
                var locationResult = await WaitForCompletionAsync(locationHandle, cancellationToken);
                if (locationResult != AddressableOperationResult.Success) return locationResult;
                if (locationHandle.Result.Count == 0) return AddressableOperationResult.Success;

                var loadHandle = Addressables.LoadAssetsAsync<Object>(locationHandle.Result, addressableAssetCache.RegisterSpriteAtlas);
                var loadResult = await WaitForCompletionAsync(loadHandle, cancellationToken);
                if (loadResult != AddressableOperationResult.Success)
                {
                    ReleaseIfValid(loadHandle);
                    return loadResult;
                }

                addressableAssetCache.CacheLabel(cacheKey, loadHandle);
                return AddressableOperationResult.Success;
            }
            finally
            {
                ReleaseIfValid(locationHandle);
            }
        }

        private void ResolveSceneSprites()
        {
            foreach (var atlasImage in FindObjectsByType<AtlasImage>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                atlasImage.ResolveSprite(GetSprite);
            }
            foreach (var spriteRenderer in FindObjectsByType<AddressableSpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                spriteRenderer.Resolve(GetSprite);
            }
        }

        private bool IsValidLabel(AssetLabelReference label) => label?.RuntimeKeyIsValid() ?? false;

        private void ReleaseIfValid(AsyncOperationHandle handle)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }

        private async UniTask<(AddressableOperationResult Result, T Value)> AwaitHandleAsync<T>(AsyncOperationHandle<T> handle, CancellationToken cancellationToken)
        {
            var result = await WaitForCompletionAsync(handle, cancellationToken);
            var value = result == AddressableOperationResult.Success ? handle.Result : default;
            ReleaseIfValid(handle);
            return (result, value);
        }

        private async UniTask<AddressableOperationResult> WaitForCompletionAsync(AsyncOperationHandle handle, CancellationToken cancellationToken)
            => GetOperationResult(handle, await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow());

        private AddressableOperationResult GetOperationResult(AsyncOperationHandle operation, bool isCanceled) => (isCanceled, operation.Status) switch
        {
            (true, _) => AddressableOperationResult.Canceled,
            (_, AsyncOperationStatus.Succeeded) => AddressableOperationResult.Success,
            _ => AddressableOperationResult.Failed,
        };
    }
}