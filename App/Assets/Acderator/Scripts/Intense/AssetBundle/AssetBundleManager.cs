using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Intense.Api;
using Intense.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using ZLinq;

namespace Intense.Asset
{
    public enum EAssetBundleErrorKind { None, NotFoundManifest, ProtocolError, ConnectionError, Canceled, Error }

    internal class AssetBundleManager : SingletonMonoBehaviour<AssetBundleManager>
    {
        private readonly Dictionary<string, AssetBundleManifestInfo> manifestInfoDict = new();
        private readonly Dictionary<string, LoadedAssetBundle> assetBundleDict = new();
        private readonly string[] assetBundleNameList = { "song/", "songselect/", "result/", "sounds/", "charts/" };
        private string BaseUrl => "";

        internal List<string> NotExistAssetBundleName
            => manifestInfoDict.AsValueEnumerable().Where(kv => assetBundleNameList.AsValueEnumerable().Any(kv.Key.StartsWith) && !Caching.IsVersionCached(new(kv.Value.BundleName, kv.Value.Hash))).Select(x => x.Key).ToList();

        public async UniTask LoadAssetsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var targets = new List<string>();
                    foreach (var kv in assetBundleDict) if (kv.Value == null) targets.Add(kv.Key);
                    if (targets.Count == 0) return;

                    if (manifestInfoDict.Count == 0)
                    {
                        using var request = UnityWebRequest.Get(await NetworkManager.Instance.GetUrl(ZString.Format("{0}/filelist.txt", BaseUrl)));
                        await request.SendWebRequest();
                        if (!request.result.EnumEquals(UnityWebRequest.Result.Success)) return;

                        foreach (var line in request.downloadHandler.text.Split('\n'))
                        {
                            var info = new AssetBundleManifestInfo(line);
                            if (!string.IsNullOrEmpty(info.BundleName)) manifestInfoDict[info.BundleName] = info;
                        }
                        if (manifestInfoDict.Count == 0) return;
                    }

                    var newFileSize = 0L;
                    foreach (var name in targets)
                    {
                        if (manifestInfoDict.TryGetValue(name, out var info) && !Caching.IsVersionCached(new(info.BundleName, info.Hash)))
                            newFileSize += info.FileSize;
                    }

                    if (newFileSize > 0)
                    {
                        if (!await DownloadSizeConfManager.Instance.OnOpenDownloadSizeConfPopupAsync(newFileSize))
                            return;

                        Loading.Instance.ShowLoading();
                        TouchControlManager.Instance.SetEventSystemEnabled(true);
                    }

                    var downloadedFileSize = 0L;
                    var allLoadedAssetBundle = AssetBundle.GetAllLoadedAssetBundles();
                    var loadedSet = new HashSet<string>(allLoadedAssetBundle.Count());
                    foreach (var b in allLoadedAssetBundle) loadedSet.Add(b.name);

                    foreach (var bundleName in targets)
                    {
                        if (!manifestInfoDict.TryGetValue(bundleName, out var info)) continue;
                        if (loadedSet.Contains(bundleName)) continue;

                        var isCached = Caching.IsVersionCached(new(info.BundleName, info.Hash));

                        using var request = UnityWebRequestAssetBundle.GetAssetBundle(await NetworkManager.Instance.GetUrl(ZString.Format("{0}/{1}", BaseUrl, bundleName)), new CachedAssetBundle(info.BundleName, info.Hash), info.Crc);
                        await request.SendWebRequest();

                        if (request.result.EnumEquals(UnityWebRequest.Result.ConnectionError)) return;
                        if (request.result.EnumEquals(UnityWebRequest.Result.ProtocolError)) return;

                        assetBundleDict[bundleName] = new LoadedAssetBundle(DownloadHandlerAssetBundle.GetContent(request));

                        if (DownloadSizeConfManager.Instance.IsProgressInactive && !isCached)
                        {
                            downloadedFileSize += info.FileSize;
                            Loading.Instance.SetDownloadFileSize(downloadedFileSize, newFileSize);
                        }
                    }
                    return;
                }
                finally
                {
                    if (DownloadSizeConfManager.Instance.IsProgressInactive)
                    {
                        await UniTask.Delay(500);
                        DownloadSizeConfManager.Instance.ResetProgressInActive();
                        DownloadSizeConfManager.Instance.ResetDownloadSizePopupNotRun();
                        Loading.Instance.ClearProgressBar();
                    }
                }
            }
        }

        private async UniTask<EAssetBundleErrorKind> GetManifestLoadedAsync()
        {
            if (manifestInfoDict.Count > 0) return default;

            using var request = UnityWebRequest.Get(await NetworkManager.Instance.GetUrl(ZString.Format("{0}/filelist.txt", BaseUrl)));
            await request.SendWebRequest();

            if (!request.result.EnumEquals(UnityWebRequest.Result.Success)) return EAssetBundleErrorKind.NotFoundManifest;

            foreach (var line in request.downloadHandler.text.Split('\n'))
            {
                var info = new AssetBundleManifestInfo(line);
                if (!string.IsNullOrEmpty(info.BundleName))
                    manifestInfoDict[info.BundleName] = info;
            }
            return default;
        }

        private async UniTask<EAssetBundleErrorKind> TryOpenDownloadPopupAsync(long fileSize)
        {
            if (fileSize <= 0) return default;

            var isConfirm = await DownloadSizeConfManager.Instance.OnOpenDownloadSizeConfPopupAsync(fileSize);
            if (!isConfirm) return EAssetBundleErrorKind.Canceled;

            Loading.Instance.ShowLoading();
            TouchControlManager.Instance.SetEventSystemEnabled(true);

            return default;
        }

        public async UniTask UnloadAssetsAsync(List<ESceneType> sceneTypes)
        {
            var targets = assetBundleDict.AsValueEnumerable().Where(kv => kv.Value != null && sceneTypes.Contains(kv.Value.SceneType)).Select(kv => kv.Key).ToList();
            foreach (var name in targets)
            {
                if (assetBundleDict.Remove(name, out var loaded))
                    await loaded.Bundle.UnloadAsync(true);
            }
        }

        public void AddLoadAssets(string bundleName)
        {
            if (!string.IsNullOrEmpty(bundleName) && !assetBundleDict.ContainsKey(bundleName))
                assetBundleDict.Add(bundleName, null);
        }

        internal async UniTask<UnityEngine.Object> GetLoadedObjectAsync(string bundleName, string assetName = null)
        {
            var bundle = assetBundleDict.GetValueOrDefault(bundleName)?.Bundle;
            if (bundle == null) return null;
            var request = bundle.LoadAssetAsync(assetName ?? bundle.GetAllAssetNames()[0]);
            await request;
            return request.asset;
        }

        public void ClearCache() => Caching.ClearCache();
    }
}