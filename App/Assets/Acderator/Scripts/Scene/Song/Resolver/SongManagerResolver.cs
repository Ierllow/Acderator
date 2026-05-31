using Intense;
using Intense.Api;
using Zenject;

namespace Song
{
    public class SongManagerResolver
    {
        [Inject] public NotesManager Notes { get; init; }
        [Inject] public NoteFactory Factory { get; init; }
        [Inject] public SoundManager Sound { get; init; }
        [Inject] public SongSoundController SongSound { get; init; }
        [Inject] internal NetworkManager Network { get; init; }
    }
}