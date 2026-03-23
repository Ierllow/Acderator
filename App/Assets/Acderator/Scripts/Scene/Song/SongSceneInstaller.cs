using Intense.Internal;
using System;
using Zenject;

namespace Song
{
    public class SongSceneInstaller : MonoInstaller<SongSceneInstaller>
    {
        public override void InstallBindings()
        {
            var songSceneContext = Container.Resolve<SceneContext>().AsSongSceneContext();
            Container.Bind<SongSceneContext>().FromInstance(songSceneContext).AsSingle();
            Container.Bind<NoteFactory>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<NotesManager>().AsSingle().WithArguments(songSceneContext.SongOption).NonLazy();
            Container.Bind<SongGameLogic>().AsSingle();
            Container.Bind<ScoreController>().AsSingle();
            Container.Bind<ComboController>().AsSingle();
            Container.Bind<HpBarController>().AsSingle();
            Container.Bind<SongResultCalculator>().AsSingle();
            Container.Bind<ScoreNumController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongParticleController>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<FrameRateController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongLoopController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteUpdateOptimizer>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteSpawnController>().FromComponentInHierarchy().AsSingle();
            Container.If(songSceneContext.IsAuto).BindInterfacesAndSelfTo<AutoFingerController>().AsSingle();
            Container.If(!songSceneContext.IsAuto).BindInterfacesAndSelfTo<FingerController>().FromComponentInHierarchy().AsSingle();
            Container.If(songSceneContext.SongMode.EnumEquals(ESongMode.Tutorial)).BindInterfacesAndSelfTo<SongTutorialLayerController>().AsSingle();
            Container.If(songSceneContext.SongMode.EnumEquals(ESongMode.Tutorial)).BindInterfacesAndSelfTo<SongTutorialStateController>().AsSingle();
            Container.Bind<SongControllerCollection>().AsSingle().WithArguments(songSceneContext.TutorialData);
            Container.Bind<SongControllerResolver>().AsSingle();
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