using Cysharp.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ZLinq;

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
            Container.BindInterfacesAndSelfTo<SongParticleController>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<FrameRateController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongLoopController>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteUpdateOptimizer>().AsSingle();
            Container.BindInterfacesAndSelfTo<NoteSpawnController>().FromComponentInHierarchy().AsSingle();
            if (songSceneContext.IsAuto) Container.BindInterfacesAndSelfTo<AutoFingerController>().AsSingle();
            else Container.BindInterfacesAndSelfTo<FingerController>().FromComponentInHierarchy().AsSingle();
            if (songSceneContext.SongMode.EnumEquals(ESongMode.Tutorial))
            {
                Container.BindInterfacesAndSelfTo<SongTutorialLayerController>().AsSingle();
                Container.BindInterfacesAndSelfTo<SongTutorialStateController>().AsSingle();
            }
            Container.Bind<SongControllerCollection>().AsSingle().WithArguments(songSceneContext.TutorialData);
            Container.Bind<SongControllerResolver>().AsSingle();
        }
    }
}