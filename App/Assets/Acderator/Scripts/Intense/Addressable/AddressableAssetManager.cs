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
    internal enum EAddressableOperationResult { Success, Canceled, Failed }

    internal class AddressableAssetManager : MonoBehaviour, IInitializable, ISceneUnloadHandler
    {
        [SerializeField] private AddressableAssetConfig addressableAssetConfigObject;

        [Inject] private readonly Loading loading;
        [Inject] private readonly AddressableAssetPopupController addressableAssetPopupController;
        [Inject] private readonly AddressableAssetCache addressableAssetCache;

        internal AddressableAssetConfig Config => addressableAssetConfigObject;

        public void Initialize() => InitializeCoreAsync(destroyCancellationToken).Forget(Debug.LogException);

        private async UniTask<EAddressableOperationResult> InitializeCoreAsync(CancellationToken cancellationToken)
        {
            AddressablesRuntimeProperties.SetPropertyValue(addressableAssetConfigObject.AssetServerUrl, addressableAssetConfigObject.AssetServerUrl.TrimEnd('/'));
            return await AwaitHandleAsync(Addressables.InitializeAsync(false), cancellationToken) switch
            {
                (var result, _) when result != EAddressableOperationResult.Success => result,
                _ => await AwaitHandleAsync(Addressables.CheckForCatalogUpdates(false), cancellationToken) switch
                {
                    (var result, _) when result != EAddressableOperationResult.Success => result,
                    (_, { Count: 0 }) => EAddressableOperationResult.Success,
                    (_, var catalogList) => (await AwaitHandleAsync(Addressables.UpdateCatalogs(true, catalogList, false), cancellationToken)).Result,
                },
            };
        }

        public async UniTask LoadAssetsAsync(ESceneType currentSceneType, CancellationToken cancellationToken)
        {
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

        public UniTask OnSceneUnloadingAsync()
        {
            addressableAssetCache.UnloadAll();
            return UniTask.CompletedTask;
        }

        internal UniTask<T> LoadAssetAsync<T>(AddressableAssetAddress address) where T : Object => LoadAssetAsync<T>(address.Value);

        internal UniTask<T> LoadAssetAsync<T>(AssetReferenceT<T> reference) where T : Object => reference == null || !reference.RuntimeKeyIsValid() ? UniTask.FromResult<T>(default) : LoadAssetAsync<T>(reference.RuntimeKey);

        private async UniTask<T> LoadAssetAsync<T>(object key) where T : Object
        {
            var cacheKey = key.ToString();
            if (addressableAssetCache.TryGetAsset<T>(cacheKey, out var cachedAsset)) return cachedAsset;

            var loadHandle = Addressables.LoadAssetAsync<Object>(key);
            var result = await WaitForCompletionAsync(loadHandle, CancellationToken.None);
            if (result != EAddressableOperationResult.Success)
            {
                if (loadHandle.IsValid()) Addressables.Release(loadHandle);
                return default;
            }

            addressableAssetCache.CacheAsset(cacheKey, loadHandle);
            return loadHandle.Result as T;
        }

        private async UniTask<EAddressableOperationResult> TryLoadAssetsAsync(ESceneType currentSceneType, CancellationToken cancellationToken)
        {
            var targetList = addressableAssetCache.GetUnloadedAddresses();
            var downloadKeyList = GetDownloadKeyList(currentSceneType, targetList);

            return await GetDownloadSizeAsync(downloadKeyList, cancellationToken) switch
            {
                (var result, _) when result != EAddressableOperationResult.Success => result,
                (_, var size) when size > 0 && !await addressableAssetPopupController.TryDownloadConfirmedAsync(size) => EAddressableOperationResult.Canceled,
                _ => await DownloadDependenciesAsync(downloadKeyList, cancellationToken) switch
                {
                    var result when result != EAddressableOperationResult.Success => result,
                    _ => await LoadAddressablesAsync(targetList, cancellationToken) switch
                    {
                        var result when result != EAddressableOperationResult.Success => result,
                        _ => await LoadLabelAssetsAsync(addressableAssetConfigObject.GetLabels(currentSceneType), cancellationToken),
                    },
                },
            };
        }

        public void AddLoad(AddressableAssetAddress address) => addressableAssetCache.AddLoad(address.Value);

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

        private UniTask<(EAddressableOperationResult Result, long Value)> GetDownloadSizeAsync(IEnumerable addressList, CancellationToken cancellationToken)
            => AwaitHandleAsync(Addressables.GetDownloadSizeAsync(addressList), cancellationToken);

        private async UniTask<EAddressableOperationResult> DownloadDependenciesAsync(IEnumerable addressList, CancellationToken cancellationToken)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(addressList, Addressables.MergeMode.Union, false);
            var isCanceled = await UniTask.WaitUntil(() =>
            {
                var status = downloadHandle.GetDownloadStatus();
                loading.SetDownloadFileSize(status.DownloadedBytes, status.TotalBytes);
                return downloadHandle.IsDone;
            }, cancellationToken: cancellationToken).SuppressCancellationThrow();
            var result = GetOperationResult(downloadHandle, isCanceled);
            if (downloadHandle.IsValid()) Addressables.Release(downloadHandle);
            return result;
        }

        private async UniTask<EAddressableOperationResult> LoadAddressablesAsync(IEnumerable<string> addressList, CancellationToken cancellationToken)
        {
            foreach (var address in addressList)
            {
                var loadHandle = Addressables.LoadAssetAsync<Object>(address);
                var result = await WaitForCompletionAsync(loadHandle, cancellationToken);
                if (result != EAddressableOperationResult.Success)
                {
                    if (loadHandle.IsValid()) Addressables.Release(loadHandle);
                    return result;
                }

                addressableAssetCache.CacheAsset(address, loadHandle);
            }
            return EAddressableOperationResult.Success;
        }

        internal Sprite GetSprite(string spriteName) => addressableAssetCache.GetSprite(spriteName);

        private async UniTask<EAddressableOperationResult> LoadLabelAssetsAsync(IEnumerable<AssetLabelReference> labelListESceneType, CancellationToken cancellationToken)
        {
            foreach (var label in labelListESceneType.Where(x => x?.RuntimeKeyIsValid() ?? false))
            {
                var cacheKey = string.Format("label:{0}", label.labelString);
                if (addressableAssetCache.IsLabelLoaded(cacheKey)) continue;

                var locationHandle = Addressables.LoadResourceLocationsAsync(label.RuntimeKey, typeof(Object));
                var locationResult = await WaitForCompletionAsync(locationHandle, cancellationToken);
                if (locationResult != EAddressableOperationResult.Success)
                {
                    if (locationHandle.IsValid()) Addressables.Release(locationHandle);
                    return locationResult;
                }
                if (locationHandle.Result.Count == 0)
                {
                    if (locationHandle.IsValid()) Addressables.Release(locationHandle);
                    continue;
                }

                var loadHandle = Addressables.LoadAssetsAsync<Object>(locationHandle.Result, addressableAssetCache.RegisterSpriteAtlas);
                var loadResult = await WaitForCompletionAsync(loadHandle, cancellationToken);
                if (loadResult != EAddressableOperationResult.Success)
                {
                    if (loadHandle.IsValid()) Addressables.Release(loadHandle);
                    if (locationHandle.IsValid()) Addressables.Release(locationHandle);
                    return loadResult;
                }

                addressableAssetCache.CacheLabel(cacheKey, loadHandle);
                if (locationHandle.IsValid()) Addressables.Release(locationHandle);
            }
            return EAddressableOperationResult.Success;
        }

        private async UniTask<(EAddressableOperationResult Result, T Value)> AwaitHandleAsync<T>(AsyncOperationHandle<T> handle, CancellationToken cancellationToken)
        {
            var result = await WaitForCompletionAsync(handle, cancellationToken);
            var value = result == EAddressableOperationResult.Success ? handle.Result : default;
            if (handle.IsValid()) Addressables.Release(handle);
            return (result, value);
        }

        private async UniTask<EAddressableOperationResult> WaitForCompletionAsync(AsyncOperationHandle handle, CancellationToken cancellationToken)
        {
            var isCanceled = await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: cancellationToken).SuppressCancellationThrow();
            return GetOperationResult(handle, isCanceled);
        }

        private EAddressableOperationResult GetOperationResult(AsyncOperationHandle operation, bool isCanceled) => (isCanceled, operation.Status) switch
        {
            (true, _) => EAddressableOperationResult.Canceled,
            (_, AsyncOperationStatus.Succeeded) => EAddressableOperationResult.Success,
            _ => EAddressableOperationResult.Failed,
        };
    }
}