#nullable enable

using Intense.Internal;
using Intense.UI;
using Zenject;

namespace Song
{
    public class SongSceneInstaller : MonoInstaller<SongSceneInstaller>
    {
        public override void InstallBindings()
        {
            var songSceneContext = Container.Resolve<SceneContext>().AsSongSceneContext();
            Container.Bind<SongSceneContext>().FromInstance(songSceneContext).AsSingle();
            BindCore(songSceneContext);
            BindSceneServices();
            BindInput();
            if (songSceneContext.SongMode == SongMode.Tutorial) BindTutorial();
        }

        private void BindCore(SongSceneContext songSceneContext)
        {
            Container.Bind<NoteFactory>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<NotesManager>().AsSingle().WithArguments(songSceneContext.SongOption).NonLazy();
            Container.Bind<SongGameLogic>().AsSingle();
            Container.Bind<ScoreController>().AsSingle();
            Container.Bind<ComboController>().AsSingle();
            Container.Bind<HpBarController>().AsSingle();
            Container.Bind<NoteJudgeController>().AsSingle();
            Container.Bind<SongSoundController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongParticleController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SongTimeCalculator>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongLoopController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NotePositionUpdater>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteSpawnController>().AsSingle();
            Container.Bind<SongAssetLoader>().AsSingle();
            Container.Bind<FailFastExceptionWatcher>().AsSingle();
        }

        private void BindSceneServices()
        {
            Container.Bind<SongControllerResolver>().AsSingle();
            Container.Bind<SongManagerResolver>().AsSingle();
        }

        private void BindInput()
        {
            Container.Bind<PointerInput>().AsSingle();
            Container.Bind<LaneDetector>().AsSingle();
            Container.Bind<TouchStateManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<FingerJudgeRequestController>().AsSingle();
            Container.BindInterfacesAndSelfTo<FingerController>().FromComponentInHierarchy().AsSingle();
        }

        private void BindTutorial()
        {
            Container.Bind<TutorialMessageResolver>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongTutorialLayerController>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<SongTutorialStateManager>().AsSingle();
        }
    }

    static class SceneContextExtensions
    {
        public static SongSceneContext AsSongSceneContext(this SceneContext sceneContext)
        {
            var songSceneContext = sceneContext as SongSceneContext;
            Error.ThrowArgumentNullException(songSceneContext, nameof(songSceneContext));
            return songSceneContext!;
        }
    }
}