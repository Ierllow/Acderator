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
        [SerializeField] private KeyAndValue<ENoteType, GameObject>[] notePrefabs;
        [SerializeField] private Transform notePoolTransform;
        [SerializeField] private KeyAndValue<ENoteType, ParticleObject>[] hitParticlePrefab;
        [SerializeField] private JudgeParticleObject judgeParticlePrefab;
        [SerializeField] private Transform particlePoolTransform;

        public override void InstallBindings()
        {
            var songSceneContext = Container.Resolve<SceneContext>().AsSongSceneContext();
            BindNotePool<SingleNote>(ENoteType.Single);
            BindNotePool<LongNote>(ENoteType.Long);
            BindNotePool<FlickNote>(ENoteType.Flick);
            BindNotePool<CurveNote>(ENoteType.Curve);
            Container.BindMemoryPool<ParticleObject, TapParticlePool>().WithInitialSize(4).FromComponentInNewPrefab(hitParticlePrefab.AsValueEnumerable().FirstOrDefault(x => x.key.EnumEquals(ENoteType.Single)).value).UnderTransform(particlePoolTransform);
            Container.BindMemoryPool<HoldParticleObject, HoldParticlePool>().WithInitialSize(4).FromComponentInNewPrefab(hitParticlePrefab.AsValueEnumerable().FirstOrDefault(x => x.key.EnumEquals(ENoteType.Long)).value).UnderTransform(particlePoolTransform);
            Container.BindMemoryPool<JudgeParticleObject, JudgeParticlePool>().WithInitialSize(4).FromComponentInNewPrefab(judgeParticlePrefab).UnderTransform(particlePoolTransform);
            Container.Bind<SongSceneContext>().FromInstance(songSceneContext).AsSingle();
            Container.Bind<NoteFactory>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<NotesManager>().AsSingle().WithArguments(songSceneContext.SongOption).NonLazy();
            Container.Bind<SongGameLogic>().AsSingle();
            Container.Bind<ScoreController>().AsSingle();
            Container.Bind<ComboController>().AsSingle();
            Container.Bind<HpBarController>().AsSingle();
            Container.Bind<SongResultCalculator>().AsSingle();
            Container.BindInterfacesAndSelfTo<SongParticleController>().AsSingle();
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

        private GameObject GetNotePrefab(ENoteType noteType)
        {
            var pair = notePrefabs.AsValueEnumerable().FirstOrDefault(x => x.key.EnumEquals(noteType));
            return pair.value == null ? throw new Exception(ZString.Format("{0} dose not exist", noteType)) : pair.value;
        }

        private void BindNotePool<TNote>(ENoteType noteType) where TNote : NoteBase => Container.BindMemoryPool<TNote, NotePool<TNote>>().WithInitialSize(10).FromComponentInNewPrefab(GetNotePrefab(noteType)).UnderTransform(notePoolTransform);
    }
}