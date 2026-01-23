using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Element.UI;
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
    public enum EFileSizeType { [Text("KB")] Kb, [Text("MB")] Mb, [Text("GB")] Gb }

    internal class AssetBundleManager : SingletonMonoBehaviour<AssetBundleManager>
    {
        private readonly Dictionary<string, AssetBundleManifestInfo> manifestInfoDict = new();
        private readonly Dictionary<string, LoadedAssetBundle> assetBundleDict = new();
        private readonly string[] assetBundleNameList = { "song/", "songselect/", "result/", "sounds/", "charts/" };
        private string BaseUrl => ""; //

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
                        Loading.Instance.ShowLoading();
                        using var request = UnityWebRequest.Get(await NetworkManager.Instance.GetUrl(ZString.Format("{0}/filelist.txt", BaseUrl)));
                        await request.SendWebRequest();
                        if (await TryOpenAssetErrorPopupAsync(request.result)) continue;

                        foreach (var line in request.downloadHandler.text.Split('\n'))
                        {
                            var info = new AssetBundleManifestInfo(line);
                            if (!string.IsNullOrEmpty(info.BundleName)) manifestInfoDict[info.BundleName] = info;
                        }
                        if (manifestInfoDict.Count == 0) return;
                        Loading.Instance.HideLoading();
                    }

                    var newFileSize = 0L;
                    foreach (var name in targets)
                    {
                        if (manifestInfoDict.TryGetValue(name, out var info) && !Caching.IsVersionCached(new(info.BundleName, info.Hash)))
                            newFileSize += info.FileSize;
                    }

                    if (newFileSize > 0)
                    {
                        if (!(await TryOpenDownloadPopupAsync(newFileSize)).EnumEquals(EAssetBundleErrorKind.None))
                            return;
                        Loading.Instance.ShowLoading();
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
                        if (await TryOpenAssetErrorPopupAsync(request.result)) continue;

                        assetBundleDict[bundleName] = new LoadedAssetBundle(DownloadHandlerAssetBundle.GetContent(request));

                        if (!isCached)
                        {
                            downloadedFileSize += info.FileSize;
                            Loading.Instance.SetDownloadFileSize(downloadedFileSize, newFileSize);
                        }
                    }
                    return;
                }
                finally
                {
                    await UniTask.Delay(500);
                    Loading.Instance.ClearProgressBar();
                }
            }
        }

        private async UniTask<EAssetBundleErrorKind> TryOpenDownloadPopupAsync(long fileSize)
        {
            if (fileSize <= 0) return default;

            PopupManager.Instance.OpenPopup(new DownloadSizeConfPopupContext { FileSize = fileSize.GetFileSize(), Size = fileSize.GetFileSizeType().GetTextAttribute().Text });
            var downloadSizeConfPopup = PopupManager.Instance.CurrentOpenPopup as DownloadSizeConfPopup;
            await UniTask.WaitUntil(() => downloadSizeConfPopup.IsClose);
            return !downloadSizeConfPopup.IsConfirm ? EAssetBundleErrorKind.Canceled : default;
        }

        private async UniTask<bool> TryOpenAssetErrorPopupAsync(UnityWebRequest.Result result)
        {
            if (!result.EnumEquals(UnityWebRequest.Result.Success))
            {
                var kind = result.EnumEquals(UnityWebRequest.Result.ProtocolError)
                    ? EAssetBundleErrorKind.ProtocolError
                    : EAssetBundleErrorKind.ConnectionError;
                return (await PopupUtils.OpenAssetErrorPopup(kind)).EnumEquals(ECommonPopupTapKind.Positive);
            }
            return false;
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

        public void AddLoadAssets(string bundleName) => assetBundleDict.TryAdd(bundleName, null);

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

    static class FileSizeExtensions
    {
        public static double GetFileSize(this long fileSize) => GetFileSizeType(fileSize) switch
        {
            EFileSizeType.Kb => Math.Round((double)fileSize / 1024, 2),
            EFileSizeType.Mb => Math.Round(fileSize / Math.Pow(1024, 2), 2),
            EFileSizeType.Gb => Math.Round(fileSize / Math.Pow(1024, 3), 2),
            _ => 0,
        };

        public static EFileSizeType GetFileSizeType(this long fileSize) => Math.Pow(1024, 2) > fileSize
            ? EFileSizeType.Kb : Math.Pow(1024, 3) > fileSize
            ? EFileSizeType.Mb : EFileSizeType.Gb;
    }
}