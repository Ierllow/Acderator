using Intense;
using Intense.Master;

namespace Song
{
    public sealed class JudgmentTypeResolver
    {
        private readonly MasterDataManager masterDataManager;

        public JudgmentTypeResolver(MasterDataManager masterDataManager) => this.masterDataManager = masterDataManager;

        public EJudgementType GetJudgmentType(float diffSec)
        {
            var zone = masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => diffSec <= x.Zone);
            return zone == default ? EJudgementType.None : (EJudgementType)zone.Type;
        }
    }
}