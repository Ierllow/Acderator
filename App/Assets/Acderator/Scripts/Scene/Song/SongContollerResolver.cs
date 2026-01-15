using System;
using System.Collections.Generic;

namespace Song
{
    public sealed class SongControllerResolver
    {
        private readonly SongControllerCollection songControllerCollection;
        private readonly Dictionary<Type, IController> cacheDict = new();

        public SongLoopController Loop => GetController<SongLoopController>();
        public NoteSpawnController Spawner => GetController<NoteSpawnController>();
        public NoteUpdateOptimizer Optimizer => GetController<NoteUpdateOptimizer>();
        public FingerController Finger => GetController<FingerController>();
        public AutoFingerController Auto => GetController<AutoFingerController>();
        public SongTutorialLayerController Tutorial => GetController<SongTutorialLayerController>();
        public SongTutorialStateController TutorialState => GetController<SongTutorialStateController>();
        public SongParticleController Particle => GetController<SongParticleController>();

        public SongControllerResolver(SongControllerCollection songControllerCollection) => this.songControllerCollection = songControllerCollection;

        private T GetController<T>() where T : class, IController => cacheDict.TryGetValue(typeof(T), out var cachedController) ? (T)cachedController : songControllerCollection.TryGet<T>(out var controller) ? controller : default;
    }
}