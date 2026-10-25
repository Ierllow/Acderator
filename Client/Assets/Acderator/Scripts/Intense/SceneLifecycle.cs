using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Intense
{
    public interface ISceneUnloadHandler
    {
        UniTask OnSceneUnloadingAsync();
    }

    public interface ISceneLoadedHandler
    {
        void OnSceneLoaded(SceneContext context);
    }

    public class UnusedAssetUnloadHandler : ISceneUnloadHandler
    {
        public async UniTask OnSceneUnloadingAsync() => await Resources.UnloadUnusedAssets();
    }

    public class FrameRateSceneLoadedHandler : ISceneLoadedHandler
    {
        public void OnSceneLoaded(SceneContext context) => Application.targetFrameRate = context.FrameRate;
    }

    public sealed class SceneLifecycleDispatcher
    {
        [Inject] private readonly List<ISceneUnloadHandler> sceneUnloadHandlers;
        [Inject] private readonly List<ISceneLoadedHandler> sceneLoadedHandlers;

        public async UniTask OnSceneUnloadingAsync()
        {
            foreach (var handler in sceneUnloadHandlers) await handler.OnSceneUnloadingAsync();
        }

        public void OnSceneLoaded(SceneContext context)
        {
            foreach (var handler in sceneLoadedHandlers) handler.OnSceneLoaded(context);
        }
    }
}