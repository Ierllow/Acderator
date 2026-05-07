using Intense.Internal;
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
            BindInput(songSceneContext);
            BindTutorial(songSceneContext);
            BindSceneServices(songSceneContext);
        }

        private void BindCore(SongSceneContext songSceneContext)
        {
            Container.Bind<NoteFactory>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<NotesManager>().AsSingle().WithArguments(songSceneContext.SongOption).NonLazy();
            Container.Bind<SongGameLogic>().AsSingle();
            Container.Bind<ScoreController>().AsSingle();
            Container.Bind<ComboController>().AsSingle();
            Container.Bind<HpBarController>().AsSingle();
            Container.Bind<JudgmentTypeResolver>().AsSingle();
            Container.Bind<SongResultCalculator>().AsSingle();
            Container.Bind<ScoreNumController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongParticleController>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<FrameRateController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongLoopController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteUpdateOptimizer>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteSpawnController>().FromComponentInHierarchy().AsSingle();
        }

        private void BindSceneServices(SongSceneContext songSceneContext)
        {
            Container.Bind<SongControllerCollection>().AsSingle().WithArguments(songSceneContext.TutorialData);
            Container.Bind<SongControllerResolver>().AsSingle();
            Container.Bind<SongAssetLoader>().AsSingle();
        }

        private void BindInput(SongSceneContext songSceneContext)
        {
            Container.If(songSceneContext.SongMode == ESongMode.Normal).Bind<SongApplicationPauseHandler>().AsSingle();
            Container.If(songSceneContext.IsAuto).BindInterfacesAndSelfTo<AutoFingerController>().AsSingle();
            Container.If(!songSceneContext.IsAuto).Bind<PointerInput>().AsSingle();
            Container.If(!songSceneContext.IsAuto).BindInterfacesAndSelfTo<FingerController>().FromComponentInHierarchy().AsSingle();
        }

        private void BindTutorial(SongSceneContext songSceneContext)
        {
            Container.If(songSceneContext.SongMode == ESongMode.Tutorial).Bind<TutorialMessageResolver>().AsSingle();
            Container.If(songSceneContext.SongMode == ESongMode.Tutorial).BindInterfacesAndSelfTo<SongTutorialLayerController>().FromComponentInHierarchy().AsSingle();
            Container.If(songSceneContext.SongMode == ESongMode.Tutorial).BindInterfacesAndSelfTo<SongTutorialStateController>().AsSingle();
        }
    }

    static class SceneContextExtensions
    {
        public static SongSceneContext AsSongSceneContext(this SceneContext sceneContext)
        {
            var songSceneContext = sceneContext as SongSceneContext;
            Error.ThrowArgumentNullException(songSceneContext, nameof(songSceneContext));
            return songSceneContext;
        }
    }
}