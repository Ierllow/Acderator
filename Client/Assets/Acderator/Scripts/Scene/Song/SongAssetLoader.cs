#nullable enable

using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
using System.Threading;
using Zenject;

namespace Song
{
    public sealed class SongAssetLoader
    {
        [Inject] private readonly AddressableAssetManager addressableAssetManager = default!;
        [Inject] private readonly SongSceneContext sceneContext = default!;

        public async UniTask LoadAssetsAsync(SceneType sceneType, CancellationToken token)
        {
            foreach (var address in sceneContext.DynamicAssetAddressList)
            {
                addressableAssetManager.AddLoad(address);
            }
            await addressableAssetManager.LoadAssetsAsync(sceneType, token);
        }
    }
}