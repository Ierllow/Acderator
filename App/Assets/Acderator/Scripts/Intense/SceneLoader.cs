using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Zenject;

namespace Intense
{
    internal sealed class SceneLoader
    {
        [Inject] private readonly ZenjectSceneLoader zenjectSceneLoader;

        public UniTask LoadAsync(ESceneType sceneType, SceneContext context) => zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), extraBindings: container => BindSceneContext(container, context)).ToUniTask();

        public UniTask LoadAdditiveAsync(ESceneType sceneType, SceneContext context) => zenjectSceneLoader.LoadSceneAsync(sceneType.ToString(), LoadSceneMode.Additive, container => BindSceneContext(container, context)).ToUniTask();

        private void BindSceneContext(DiContainer container, SceneContext context) => container.Bind<SceneContext>().FromInstance(context).AsSingle();
    }
}