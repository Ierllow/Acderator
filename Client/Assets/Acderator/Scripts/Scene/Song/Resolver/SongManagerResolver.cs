#nullable enable

using Intense.Api;
using Intense.Asset;
using Zenject;

namespace Song
{
    public class SongManagerResolver
    {
        [Inject] public readonly NotesManager Notes = default!;
        [Inject] public readonly NoteFactory Factory = default!;
        [InjectOptional] public readonly SongTutorialStateManager? TutorialState = default;
        [Inject] internal readonly NetworkManager Network = default!;
        [Inject] internal readonly AddressableAssetManager Addressable = default!;
    }
}