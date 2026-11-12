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

        private readonly OrderType[] orderTypes;
        private readonly Dictionary<OrderType, string> orderTypeTextDict;

        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly ScoreManager scoreManager;

        public OrderType CurrentOrderType { get; private set; } = (OrderType)PlayerPrefsValues.OrderType;

        public string Text => orderTypeTextDict.TryGetValue(CurrentOrderType, out var text) ? text : CurrentOrderType.ToString();

        public SongSelectSortController()
        {
            orderTypes = (OrderType[])Enum.GetValues(typeof(OrderType));
            orderTypeTextDict = orderTypes.ToDictionary(x => x, GetOrderTypeText);
        }

        public List<int> GetOrderedList(int selectedDifficulty)
        {
            var songs = masterDataManager.MemoryDatabase.SongMasterTable.All;
            IEnumerable<SongMaster> orderedSongs = CurrentOrderType switch
            {
                OrderType.Level => songs.OrderBy(x => x.Difficulty == selectedDifficulty),
                OrderType.HighScore => songs.OrderBy(x => -scoreManager.GetScore(x.Sid)),
                OrderType.Name => songs.OrderBy(x => x.Name),
                _ => songs,
            };
            return orderedSongs.Select(x => x.Group).Distinct().ToList();
        }

        public void SetNextOrderType()
        {
            var index = (int)CurrentOrderType + 1;
            CurrentOrderType = index < orderTypes.Length ? orderTypes[index] : default;
        }

        public void SaveOrderType() => PlayerPrefsValues.Set(PlayerPrefsKey.OrderType, (int)CurrentOrderType);

        private string GetOrderTypeText(OrderType orderType)
        {
            var name = orderType.ToString();
            return typeof(OrderType).GetField(name)?.GetCustomAttribute<TextAttribute>()?.Text ?? name;
        }
    }
}