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

        public SoundSheetNameMaster FindBgm(EBgmType type)
        {
            var id = type switch
            {
                EBgmType.GameResult => 2,
                EBgmType.GameResultFailed => 3,
                _ => (int)type,
            };
            return masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == BgmCategory && x.Id == id);
        }

        public SoundSheetNameMaster FindSe(ESeType type)
        {
            var id = type switch
            {
                ESeType.Tap => 0,
                ESeType.Flick => 1,
                _ => (int)type,
            };
            return masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == SeCategory && x.Id == id);
        }
    }
}