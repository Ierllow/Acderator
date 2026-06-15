#nullable enable

using Intense;
using Intense.Master;

namespace Song
{
    public sealed class SongSoundSheetNameResolver : SoundSheetNameResolverBase
    {
        private const int SongCategory = 1;

        public SoundSheetNameMaster Song => FindByCategory(SongCategory);
    }
}