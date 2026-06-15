using Intense.UI;
using Zenject;

namespace Title
{
    public class TitleSceneInstaller : MonoInstaller<TitleSceneInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<FailFastExceptionWatcher>().AsSingle();
            Container.Bind<TitleAuthController>().AsSingle();
            Container.Bind<Song.TutorialSceneContextBuilder>().AsSingle();
        }
    }
}