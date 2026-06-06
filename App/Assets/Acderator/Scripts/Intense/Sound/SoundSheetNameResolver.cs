using Intense.Master;

namespace Intense
{
    public sealed class SoundSheetNameResolver : SoundSheetNameResolverBase
    {
        private const int BgmCategory = 0;
        private const int SeCategory = 3;

        public SoundSheetNameMaster FindBgm(EBgmType type) => FindByCategoryAndId(BgmCategory, (int)type);

        public SoundSheetNameMaster FindSe(ESeType type) => FindByCategoryAndId(SeCategory, (int)type);
    }
}