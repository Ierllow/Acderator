using Cysharp.Threading.Tasks;

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
}