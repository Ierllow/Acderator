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

        public const int MaxHpPercent = 100;

        public void Init(int sid) => baseHp = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sid).Hp;

        public void UpdateHp(JudgementType judgmentType, int noteCount)
        {
            if (judgmentType == JudgementType.Bad || judgmentType == JudgementType.Miss) return;
            if (currentHpNum >= baseHp) return;

            var rate = masterDataManager.MemoryDatabase.SongHpRateMasterTable.FindByType((int)judgmentType).Rate;
            currentHpNum += judgmentType switch
            {
                JudgementType.Perfect or JudgementType.Great or JudgementType.Good => Mathf.FloorToInt(rate / noteCount),
                _ => -Mathf.FloorToInt(rate / noteCount),
            };
            currentHpNum = Mathf.Clamp(currentHpNum, 0, baseHp);
            currentHpPercent.Value = (float)Math.Round((decimal)((double)currentHpNum / baseHp), 4, MidpointRounding.AwayFromZero);
        }
    }
}