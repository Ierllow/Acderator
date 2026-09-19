using Intense.Attribute;
using Intense.Data;
using Intense.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using Zenject;

namespace SongSelect
{
    public class SongSelectSortController
    {
        public enum EOrderType
        {
            [Text("デフォルト")] Default,
            [Text("楽曲名")] Name,
            [Text("レベル")] Level,
            [Text("ハイスコア")] HighScore,
        }

        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly ScoreManager scoreManager;

        public EOrderType CurrentOrderType { get; private set; } = (EOrderType)PlayerPrefsValues.OrderType;

        public string Text => typeof(EOrderType).GetField(CurrentOrderType.ToString())?.GetCustomAttribute<TextAttribute>()?.Text ?? CurrentOrderType.ToString();

        public List<int> GetOrderedList(int selectedDifficulty)
        {
            var songMasterTables = masterDataManager.MemoryDatabase.SongMasterTable;
            return CurrentOrderType switch
            {
                EOrderType.Default => songMasterTables.Select(x => x.Group).Distinct().ToList(),
                EOrderType.Level => songMasterTables.OrderBy(x => x.Difficulty == selectedDifficulty).Select(x => x.Group).Distinct().ToList(),
                EOrderType.HighScore => songMasterTables.OrderBy(x => -scoreManager.GetScore(x.Sid)).Select(x => x.Group).Distinct().ToList(),
                EOrderType.Name => songMasterTables.OrderBy(x => x.Name).Select(x => x.Group).Distinct().ToList(),
                _ => default
            };
        }

        public void UpdateOrderType(EOrderType orderType) => CurrentOrderType = orderType;

        public void SetNextOrderType() => CurrentOrderType = ((EOrderType[])Enum.GetValues(typeof(EOrderType))).ElementAtOrDefault((int)CurrentOrderType + 1);

        public void SaveOrderType() => PlayerPrefsValues.Set(PlayerPrefsKey.OrderType, (int)CurrentOrderType);
    }
}