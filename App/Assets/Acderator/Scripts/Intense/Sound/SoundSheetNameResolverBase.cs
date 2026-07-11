using Intense.Master;
using Zenject;

namespace Intense
{
    public abstract class SoundSheetNameResolverBase
    {
        [Inject] private readonly MasterDataManager masterDataManager;

        protected SoundSheetNameMaster FindByCategory(int category) => masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.FindByCategory(category);

        protected SoundSheetNameMaster FindByCategoryAndId(int category, int id) => masterDataManager.MemoryDatabase.SoundSheetNameMasterTable.First(x => x.Category == category && x.Id == id);
    }
}