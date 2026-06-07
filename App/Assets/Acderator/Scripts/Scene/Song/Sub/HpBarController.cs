#nullable enable

using Intense;
using Intense.Master;
using R3;
using System;
using UnityEngine;
using Zenject;

namespace Song
{
    public class HpBarController
    {
        [Inject] private readonly MasterDataManager masterDataManager = default!;

        private int currentHpNum;
        private int baseHp;
        private readonly ReactiveProperty<float> currentHpPercent = new(0f);

        public ReadOnlyReactiveProperty<float> CurrentHpPercent => currentHpPercent;

        public const int MAX_HP_PERCENT = 100;

        public void Init(int sid) => baseHp = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sid).Hp;

        public void UpdateHp(EJudgementType judgmentType, int noteCount)
        {
            if (judgmentType == EJudgementType.Bad || judgmentType == EJudgementType.Miss) return;
            if (currentHpNum >= baseHp) return;
            if (currentHpPercent.Value >= MAX_HP_PERCENT) return;

            var rate = masterDataManager.MemoryDatabase.SongHpRateMasterTable.FindByType((int)judgmentType).Rate;
            currentHpNum += judgmentType switch
            {
                EJudgementType.Perfect or EJudgementType.Great or EJudgementType.Good => Mathf.FloorToInt(rate / noteCount),
                _ => -Mathf.FloorToInt(rate / noteCount),
            };
            currentHpPercent.Value = (float)Math.Round((decimal)((double)currentHpNum / baseHp), 4, MidpointRounding.AwayFromZero);
        }
    }
}
