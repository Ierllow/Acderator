using Zenject;

namespace Song
{
    public sealed class SongControllerResolver
    {
        [Inject] public SongLoopController Loop;
        [Inject] public NoteSpawnController Spawner;
        [Inject] public NotePositionUpdater PositionUpdater;
        [Inject] public SongParticleController Particle;
        [Inject] public FingerController Finger;
        [Inject] public SongTutorialLayerController Tutorial;
        [Inject] public SongTutorialStateController TutorialState;
    }
}