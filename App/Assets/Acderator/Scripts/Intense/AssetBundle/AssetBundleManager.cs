using Cysharp.Threading.Tasks;
using Element.UI;
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
        [SerializeField] private NetworkConfig networkConfigObject;

        [Inject] private Loading loading;
        [Inject] private PopupManager popupManager;

        private readonly Dictionary<string, AssetBundleManifestInfo> manifestInfoDict = new();
        private readonly Dictionary<string, LoadedAssetBundle> assetBundleDict = new();
        private readonly string[] assetBundleNameList = { "song/", "songselect/", "result/", "sounds/", "charts/" };

        internal List<string> NotExistAssetBundleName
            => manifestInfoDict.Where(kv => assetBundleNameList.Any(kv.Key.StartsWith) && !Caching.IsVersionCached(new(kv.Value.BundleName, kv.Value.Hash))).Select(x => x.Key).ToList();

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
                        loading.ShowLoading();
                        using var request = UnityWebRequest.Get(string.Format("{0}/filelist.txt", networkConfigObject.assetServerUrl));
                        await request.SendWebRequest();
                        if (request.result != UnityWebRequest.Result.Success && await TryRetryAssetErrorAsync(request.result))
                            continue;

                        foreach (var line in request.downloadHandler.text.Split('\n'))
                        {
                            var info = new AssetBundleManifestInfo(line);
                            if (!string.IsNullOrEmpty(info.BundleName)) manifestInfoDict[info.BundleName] = info;
                        }
                        if (manifestInfoDict.Count == 0) return;
                        loading.HideLoading();
                    }

                    var newFileSize = 0L;
                    foreach (var name in targetList)
                    {
                        if (manifestInfoDict.TryGetValue(name, out var info) && !Caching.IsVersionCached(new(info.BundleName, info.Hash)))
                            newFileSize += info.FileSize;
                    }

                    if (newFileSize > 0)
                    {
                        if (!await TryDownloadConfirmedAsync(newFileSize)) return;
                        loading.ShowLoading();
                    }

                    var downloadedFileSize = 0L;
                    var allLoadedAssetBundle = AssetBundle.GetAllLoadedAssetBundles();
                    var loadedSet = allLoadedAssetBundle.Select(x => x.name).ToHashSet();
                    foreach (var bundleName in targetList)
                    {
                        if (!manifestInfoDict.TryGetValue(bundleName, out var info)) continue;
                        if (loadedSet.Contains(bundleName)) continue;

                        var isCached = Caching.IsVersionCached(new(info.BundleName, info.Hash));

                        using var request = UnityWebRequestAssetBundle.GetAssetBundle(string.Format("{0}/{1}", networkConfigObject.assetServerUrl, bundleName), new CachedAssetBundle(info.BundleName, info.Hash), info.Crc);
                        await request.SendWebRequest();
                        if (request.result != UnityWebRequest.Result.Success && await TryRetryAssetErrorAsync(request.result))
                            continue;

                        assetBundleDict[bundleName] = new LoadedAssetBundle { SceneType = currentSceneType, Bundle = DownloadHandlerAssetBundle.GetContent(request) };

                        if (!isCached)
                        {
                            downloadedFileSize += info.FileSize;
                            loading.SetDownloadFileSize(downloadedFileSize, newFileSize);
                        }
                    }
                    return;
                }
                finally
                {
                    await UniTask.Delay(500);
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
            var request = bundle?.LoadAssetAsync(assetName ?? bundle.GetAllAssetNames()[0]);
            return await request != null ? request.asset : default;
        }

        private async UniTask<bool> TryRetryAssetErrorAsync(UnityWebRequest.Result result)
        {
            var completionSource = AutoResetUniTaskCompletionSource<ECommonPopupTapKind>.Create();
            var kind =result == UnityWebRequest.Result.ProtocolError
                ? EAssetBundleErrorKind.ProtocolError
                : EAssetBundleErrorKind.ConnectionError;
            var popupContext = PopupContextFactory.CreateAssetErrorPopupContext(completionSource, kind);
            popupManager.OpenPopup(popupContext);
            return await completionSource.Task == ECommonPopupTapKind.Positive;
        }

        private async UniTask<bool> TryDownloadConfirmedAsync(long fileSize)
        {
            if (fileSize <= 0) return true;

            var context = PopupContextFactory.CreateDownloadSizeConfirmPopupContext(fileSize.GetFileSize(), fileSize.GetFileSizeType().GetType().GetCustomAttribute<TextAttribute>().Text);
            popupManager.OpenPopup(context);
            var downloadSizeConfPopup = popupManager.CurrentOpenPopup as DownloadSizeConfPopup;
            await UniTask.WaitUntil(() => (popupManager.CurrentOpenPopup as DownloadSizeConfPopup).IsClose);
            return downloadSizeConfPopup.IsConfirm;
        }
    }

    static class FileSizeExtensions
    {
        private const double One_KB = 1024d;
        private const long One_MB = 1024L * 1024;
        private const long One_GB = 1024L * 1024 * 1024;

        public static double GetFileSize(this long fileSize)
        {
            return GetFileSizeType(fileSize) switch
            {
                EFileSizeType.KB => Math.Round(fileSize / One_KB, 2),
                EFileSizeType.MB => Math.Round((double)fileSize / One_MB, 2),
                EFileSizeType.GB => Math.Round((double)fileSize / One_GB, 2),
                _ => 0
            };
        }

        public static EFileSizeType GetFileSizeType(this long fileSize) => fileSize < One_MB ? EFileSizeType.KB : fileSize < One_GB ? EFileSizeType.MB : EFileSizeType.GB;
    }
}