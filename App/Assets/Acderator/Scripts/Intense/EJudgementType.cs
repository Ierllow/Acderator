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
        public static EJudgementType GetJudgmentType(this float diffSec)
        {
            var zone = MasterDataManager.Instance.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => diffSec <= x.Zone);
            return zone == default ? EJudgementType.None : (EJudgementType)zone.Type;
        }
    }
}