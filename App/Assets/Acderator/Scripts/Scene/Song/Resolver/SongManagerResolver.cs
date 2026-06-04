using Intense.Api;
using Zenject;

namespace Song
{
    public class SongManagerResolver
    {
        [Inject] public readonly NotesManager Notes;
        [Inject] public readonly NoteFactory Factory;
        [Inject] public readonly SongTutorialStateManager TutorialState;
        [Inject] internal readonly NetworkManager Network;
    }
}