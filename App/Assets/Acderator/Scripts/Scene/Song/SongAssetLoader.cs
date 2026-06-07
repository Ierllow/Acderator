#nullable enable

using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Song
{
    public sealed class SongLoadResult
    {
        public bool IsSuccess => LoadResult == ELoadResult.None;
        public ELoadResult LoadResult { get; init; }
        public LoadedChartInfo ChartInfo { get; init; } = default!;
    }

    public sealed class SongAssetLoader
    {
        [Inject] private readonly AddressableAssetManager addressableAssetManager = default!;
        [Inject] private readonly SongSceneContext sceneContext = default!;

        public async UniTask LoadAssetsAsync(ESceneType sceneType, CancellationToken token)
        {
            foreach (var address in sceneContext.DynamicAssetAddressList)
            {
                addressableAssetManager.AddLoad(address);
            }
            await addressableAssetManager.LoadAssetsAsync(sceneType, token);
        }

        public async UniTask<SongLoadResult> LoadChart()
        {
            var loadedChartInfo = new LoadedChartInfo();

            var chartAsset = await addressableAssetManager.LoadAssetAsync<TextAsset>(sceneContext.SongChartAddress);
            if (chartAsset is not TextAsset textAsset)
            {
                return new SongLoadResult { LoadResult = ELoadResult.InvalidAsset, ChartInfo = loadedChartInfo };
            }
            await new ChartLoader().LoadChart(textAsset.text, loadedChartInfo);
            return new SongLoadResult { LoadResult = ELoadResult.None, ChartInfo = loadedChartInfo };
        }
    }
}
