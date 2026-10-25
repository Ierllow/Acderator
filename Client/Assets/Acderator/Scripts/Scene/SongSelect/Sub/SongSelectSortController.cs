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
        public enum OrderType
        {
            [Text("デフォルト")] Default,
            [Text("楽曲名")] Name,
            [Text("レベル")] Level,
            [Text("ハイスコア")] HighScore,
        }

        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly ScoreManager scoreManager;

        public OrderType CurrentOrderType { get; private set; } = (OrderType)PlayerPrefsValues.OrderType;

        public string Text => typeof(OrderType).GetField(CurrentOrderType.ToString())?.GetCustomAttribute<TextAttribute>()?.Text ?? CurrentOrderType.ToString();

        public List<int> GetOrderedList(int selectedDifficulty)
        {
            var songMasterTables = masterDataManager.MemoryDatabase.SongMasterTable;
            return CurrentOrderType switch
            {
                OrderType.Default => songMasterTables.Select(x => x.Group).Distinct().ToList(),
                OrderType.Level => songMasterTables.OrderBy(x => x.Difficulty == selectedDifficulty).Select(x => x.Group).Distinct().ToList(),
                OrderType.HighScore => songMasterTables.OrderBy(x => -scoreManager.GetScore(x.Sid)).Select(x => x.Group).Distinct().ToList(),
                OrderType.Name => songMasterTables.OrderBy(x => x.Name).Select(x => x.Group).Distinct().ToList(),
                _ => default
            };
        }

        public void UpdateOrderType(OrderType orderType) => CurrentOrderType = orderType;

        public void SetNextOrderType() => CurrentOrderType = ((OrderType[])Enum.GetValues(typeof(OrderType))).ElementAtOrDefault((int)CurrentOrderType + 1);

        public void SaveOrderType() => PlayerPrefsValues.Set(PlayerPrefsKey.OrderType, (int)CurrentOrderType);
    }
}