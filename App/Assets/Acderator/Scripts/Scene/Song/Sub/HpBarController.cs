using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Master;
using System;
using UnityEngine;
using Zenject;

namespace Song
{
    public class HpBarController
    {
        [Inject] private MasterDataManager masterDataManager;

        private int currentHpNum;
        private decimal currentHpPercent;
        private int baseHp;

        public float CurrentHpPercent => (float)currentHpPercent;

        public IUniTaskAsyncEnumerable<float> EveryUpdateHpPercentAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(this, x => (float)x.currentHpPercent);

        public const int MAX_HP_PERCENT = 100;

        public void Init(int sid) => baseHp = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sid).Hp;

        public void UpdateHp(EJudgementType judgmentType, int noteCount)
        {
            if (judgmentType.EnumEquals(EJudgementType.Bad) || judgmentType.EnumEquals(EJudgementType.Miss)) return;
            if (currentHpNum >= baseHp) return;
            if (currentHpPercent >= MAX_HP_PERCENT) return;

            var rate = masterDataManager.MemoryDatabase.SongHpRateMasterTable.FindByType(judgmentType.GetLength()).Rate;
            currentHpNum += judgmentType switch
            {
                EJudgementType.Perfect or EJudgementType.Great or EJudgementType.Good => Mathf.FloorToInt(rate / noteCount),
                _ => -Mathf.FloorToInt(rate / noteCount),
            };
            currentHpPercent = Math.Round((decimal)((double)currentHpNum / baseHp), 4, MidpointRounding.AwayFromZero);
        }
    }
}
