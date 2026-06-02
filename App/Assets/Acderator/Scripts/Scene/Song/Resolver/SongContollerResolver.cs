using Zenject;

namespace Song
{
    public sealed class SongControllerResolver
    {
        [Inject] public readonly SongLoopController Loop;
        [Inject] public readonly NoteSpawnController Spawner;
        [Inject] public readonly NotePositionUpdater PositionUpdater;
        [Inject] public readonly SongParticleController Particle;
        [Inject] public readonly FingerController Finger;
        [Inject] public readonly FingerJudgeRequestController FingerJudgeRequest;
        [Inject] public readonly SongTutorialLayerController Tutorial;
        [Inject] public readonly SongTutorialStateController TutorialState;
    }
}