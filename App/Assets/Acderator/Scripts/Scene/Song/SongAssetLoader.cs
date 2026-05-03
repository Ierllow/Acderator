using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Song
{
    public sealed class SongLoadResult
    {
        public bool IsSuccess => LoadResult == ELoadResult.None;
        public ELoadResult LoadResult { get; init; }
        public LoadedChartInfo ChartInfo { get; init; }
    }

    public sealed class SongAssetLoader
    {
        [Inject] private readonly AssetBundleManager assetBundleManager;
        [Inject] private readonly SongSceneContext sceneContext;

        public async UniTask LoadBundles(ESceneType sceneType, CancellationToken token)
        {
            foreach (var path in sceneContext.SongBundlePathList)
            {
                assetBundleManager.AddLoadAssets(path);
            }
            await assetBundleManager.LoadAssetsAsync(sceneType, token);
        }

        public async UniTask<SongLoadResult> LoadChart()
        {
            var loadedChartInfo = new LoadedChartInfo();

            var chartAsset = await assetBundleManager.GetLoadedObjectAsync(sceneContext.SongChartBundlePath);
            if (chartAsset is not TextAsset textAsset)
            {
                return new SongLoadResult { LoadResult = ELoadResult.InvalidAsset, ChartInfo = loadedChartInfo };
            }
            await new ChartLoader().LoadChart(textAsset.text, loadedChartInfo);
            return new SongLoadResult { LoadResult = ELoadResult.None, ChartInfo = loadedChartInfo };
        }
    }
}