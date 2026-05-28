using Cysharp.Threading.Tasks;
using Intense.Api;
using Intense.Attribute;
using Intense.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace Intense.Asset
{
    public enum EAssetBundleErrorKind { None, NotFoundManifest, ProtocolError, ConnectionError, Canceled, Error }
    internal enum EFileSizeType { [Text("KB")] KB, [Text("MB")] MB, [Text("GB")] GB }

    internal class AssetBundleManager : MonoBehaviour
    {
        private enum EManifestLoadResult { Success, Retry, Empty }

        private const string ManifestFileName = "";
        private const int LoadCoolDownMs = 500;

        [SerializeField] private NetworkConfig networkConfigObject;

        [Inject] private readonly Loading loading;
        [Inject] private readonly AssetBundlePopupController assetBundlePopupController;

        private readonly Dictionary<string, AssetBundleManifestInfo> manifestInfoDict = new();
        private readonly Dictionary<string, LoadedAssetBundle> assetBundleDict = new();
        private readonly string[] assetBundleNameList = { "song/", "songselect/", "result/", "sounds/", "charts/" };

        internal List<string> NotExistAssetBundleName => manifestInfoDict.Where(kv => assetBundleNameList.Any(kv.Key.StartsWith) && !IsCached(kv.Value)).Select(kv => kv.Key).ToList();

        public async UniTask LoadAssetsAsync(ESceneType currentSceneType, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var targetList = assetBundleDict.Where(kv => kv.Value == null).Select(kv => kv.Key).ToList();
                    if (targetList.Count == 0) return;

                    if (manifestInfoDict.Count == 0)
                    {
                        switch (await LoadManifestAsync())
                        {
                            case EManifestLoadResult.Retry: continue;
                            case EManifestLoadResult.Empty: return;
                        }
                    }

                    var newFileSize = CalculateDownloadSize(targetList);
                    if (newFileSize > 0)
                    {
                        if (!await assetBundlePopupController.TryDownloadConfirmedAsync(newFileSize)) return;
                        loading.ShowLoading();
                    }

                    await DownloadBundlesAsync(targetList, currentSceneType, newFileSize);
                    return;
                }
                finally
                {
                    await UniTask.Delay(LoadCoolDownMs);
                    loading.ClearProgressBar();
                }
            }
        }

        public async UniTask UnloadAssetsAsync(List<ESceneType> sceneTypes)
        {
            var targets = assetBundleDict.Where(kv => kv.Value != null && sceneTypes.Contains(kv.Value.SceneType)).Select(kv => kv.Key).ToList();
            foreach (var name in targets)
            {
                if (assetBundleDict.Remove(name, out var loaded))
                    await loaded.Bundle.UnloadAsync(true);
            }
        }

        public void AddLoadAssets(string bundleName) => assetBundleDict.TryAdd(bundleName, null);

        internal async UniTask<UnityEngine.Object> GetLoadedObjectAsync(string bundleName, string assetName = null)
        {
            var bundle = assetBundleDict.GetValueOrDefault(bundleName)?.Bundle;
            if (bundle == null) return null;

            var request = bundle.LoadAssetAsync(assetName ?? bundle.GetAllAssetNames()[0]);
            await request;
            return request.asset;
        }

        private async UniTask<EManifestLoadResult> LoadManifestAsync()
        {
            loading.ShowLoading();

            using var request = UnityWebRequest.Get(BuildUrl(ManifestFileName));
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success && await assetBundlePopupController.TryRetryAssetErrorAsync(request.result))
                return EManifestLoadResult.Retry;

            foreach (var line in request.downloadHandler.text.Split('\n'))
            {
                var info = new AssetBundleManifestInfo(line);
                if (!string.IsNullOrEmpty(info.BundleName)) manifestInfoDict[info.BundleName] = info;
            }
            if (manifestInfoDict.Count == 0) return EManifestLoadResult.Empty;

            loading.HideLoading();
            return EManifestLoadResult.Success;
        }

        private long CalculateDownloadSize(IEnumerable<string> targetList)
        {
            var newFileSize = 0L;
            foreach (var name in targetList)
            {
                if (manifestInfoDict.TryGetValue(name, out var info) && !IsCached(info))
                    newFileSize += info.FileSize;
            }
            return newFileSize;
        }

        private async UniTask DownloadBundlesAsync(IEnumerable<string> targetList, ESceneType currentSceneType, long newFileSize)
        {
            var downloadedFileSize = 0L;
            var loadedSet = AssetBundle.GetAllLoadedAssetBundles().Select(x => x.name).ToHashSet();

            foreach (var bundleName in targetList)
            {
                if (!manifestInfoDict.TryGetValue(bundleName, out var info)) continue;
                if (loadedSet.Contains(bundleName)) continue;

                var isCached = IsCached(info);

                using var request = UnityWebRequestAssetBundle.GetAssetBundle(BuildUrl(bundleName), ToCachedBundle(info), info.Crc);
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success && await assetBundlePopupController.TryRetryAssetErrorAsync(request.result))
                    continue;

                assetBundleDict[bundleName] = new() { SceneType = currentSceneType, Bundle = DownloadHandlerAssetBundle.GetContent(request) }
                ;

                if (!isCached)
                {
                    downloadedFileSize += info.FileSize;
                    loading.SetDownloadFileSize(downloadedFileSize, newFileSize);
                }
            }
        }

        private string BuildUrl(string path) => string.Format("{0}/{1}", networkConfigObject.assetServerUrl, path);

        private bool IsCached(AssetBundleManifestInfo info) => Caching.IsVersionCached(ToCachedBundle(info));

        private CachedAssetBundle ToCachedBundle(AssetBundleManifestInfo info) => new(info.BundleName, info.Hash);
    }

    static class FileSizeExtensions
    {
        private const double One_KB = 1024d;
        private const long One_MB = 1024L * 1024;
        private const long One_GB = 1024L * 1024 * 1024;

        public static double GetFileSize(this long fileSize) => GetFileSizeType(fileSize) switch
        {
            EFileSizeType.KB => Math.Round(fileSize / One_KB, 2),
            EFileSizeType.MB => Math.Round((double)fileSize / One_MB, 2),
            EFileSizeType.GB => Math.Round((double)fileSize / One_GB, 2),
            _ => 0
        };

        public static EFileSizeType GetFileSizeType(this long fileSize) => fileSize < One_MB ? EFileSizeType.KB : fileSize < One_GB ? EFileSizeType.MB : EFileSizeType.GB;

        public static string GetText(this EFileSizeType type) => typeof(EFileSizeType).GetField(type.ToString())?.GetCustomAttribute<TextAttribute>()?.Text;
    }
}