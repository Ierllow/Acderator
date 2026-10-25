#nullable enable

using Zenject;

namespace Song
{
    public sealed class SongControllerResolver
    {
        [Inject] public readonly SongLoopController Loop = default!;
        [Inject] public readonly SongSoundController Sound = default!;
        [Inject] public readonly NoteSpawnController Spawner = default!;
        [Inject] public readonly NotePositionUpdater PositionUpdater = default!;
        [Inject] public readonly SongParticleController Particle = default!;
        [Inject] public readonly FingerController Finger = default!;
        [Inject] public readonly FingerJudgeRequestController FingerJudgeRequest = default!;
        [InjectOptional] public readonly SongTutorialLayerController? Tutorial = default;
    }
}