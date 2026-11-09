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

        private float currentHp;
        private int baseHp;
        private readonly ReactiveProperty<float> currentHpPercent = new(0f);

        public ReadOnlyReactiveProperty<float> CurrentHpPercent => currentHpPercent;

        public const int MaxHpPercent = 100;

        public void Init(int sid) => baseHp = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sid)?.Hp ?? 0;

        public void UpdateHp(JudgementType judgmentType, int noteCount)
        {
            if (noteCount <= 0 || baseHp <= 0) return;

            var hpRateMaster = masterDataManager.MemoryDatabase.SongHpRateMasterTable.FindByType((int)judgmentType);
            if (hpRateMaster == null) return;

            var delta = hpRateMaster.Rate / (float)noteCount;
            currentHp += judgmentType switch
            {
                JudgementType.Perfect or JudgementType.Great or JudgementType.Good => delta,
                JudgementType.Bad or JudgementType.Miss => -delta,
                _ => 0f,
            };
            currentHp = Mathf.Clamp(currentHp, 0, baseHp);
            currentHpPercent.Value = (float)Math.Round((decimal)((double)currentHp / baseHp), 4, MidpointRounding.AwayFromZero);
        }
    }
}