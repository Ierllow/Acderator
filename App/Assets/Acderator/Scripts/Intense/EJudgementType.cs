using Intense.Master;

namespace Intense
{
    public enum EJudgementType
    {
        None,
        Perfect,
        Great,
        Good,
        Bad,
        Miss,
    }

    public static class JudgementTypeExtensions
    {
        internal static EJudgementType GetJudgmentType(this float diffSec, MasterDataManager masterDataManager)
        {
            var zone = masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => diffSec <= x.Zone);
            return zone == default ? EJudgementType.None : (EJudgementType)zone.Type;
        }
    }
}
