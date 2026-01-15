using Intense.Internal;
using Zenject;

namespace Result
{
    public class ResultSceneInstaller : MonoInstaller<ResultSceneInstaller>
    {
        public override void InstallBindings()
        {
            var resultSceneContext = Container.Resolve<SceneContext>() as ResultSceneContext;
            Container.Bind<ResultSceneContext>().FromInstance(resultSceneContext).AsSingle();
            Error.ThrowArgumentNullException(resultSceneContext, nameof(resultSceneContext));
        }
    }
}