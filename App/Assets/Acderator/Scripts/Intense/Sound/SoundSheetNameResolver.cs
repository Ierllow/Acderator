using Intense.Master;
using Zenject;

namespace Intense
{
    public sealed class SoundSheetNameResolver
    {
        private const int BgmCategory = 0;
        private const int SongCategory = 1;
        private const int SeCategory = 3;

        [Inject] private readonly MasterDataManager masterDataManager;

        public SoundSheetNameMaster Song => masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == SongCategory);

        public SoundSheetNameMaster FindBgm(EBgmType type) => masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == BgmCategory && x.Id == (int)type);

        public SoundSheetNameMaster FindSe(ESeType type) => masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == SeCategory && x.Id == (int)type);
    }
}