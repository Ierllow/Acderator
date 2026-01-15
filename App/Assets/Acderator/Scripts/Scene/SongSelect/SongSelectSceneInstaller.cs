using Intense.Internal;
using Zenject;

namespace SongSelect
{
    public class SongSelectSceneInstaller : MonoInstaller<SongSelectSceneInstaller>
    {
        public override void InstallBindings()
        {
            var songSelectSceneContext = Container.Resolve<SceneContext>() as SongSelectSceneContext;
            Container.Bind<SongSelectSceneContext>().FromInstance(songSelectSceneContext).AsSingle();
            Error.ThrowArgumentNullException(songSelectSceneContext, nameof(songSelectSceneContext));
            Container.Bind<SongSelectSortController>().AsSingle();
            Container.Bind<SongSelectCellListController>().AsSingle();
        }
    }
}