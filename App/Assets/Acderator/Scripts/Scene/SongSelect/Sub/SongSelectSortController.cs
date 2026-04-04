using FastEnumUtility;
using Intense.Attribute;
using Intense.Data;
using Intense.Master;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using ZLinq;
using Zenject;

namespace SongSelect
{
    public class SongSelectSortController
    {

        [Inject] private MasterDataManager masterDataManager;
        [Inject] private ScoreManager scoreManager;
        public enum EOrderType
        {
            [Text("デフォルト")] Default,
            [Text("楽曲名")] Name,
            [Text("レベル")] Level,
            [Text("ハイスコア")] HighScore,
        }

        public EOrderType CurrentOrderType { get; private set; } = (EOrderType)PlayerPrefsValues.OT;

        public string Text => CurrentOrderType.GetType().GetCustomAttribute<TextAttribute>().Text;

        public List<int> GetOrderedList(int selectedDifficulty)
        {
            var songMasterTables = masterDataManager.MemoryDatabase.SongMasterTable;
            return CurrentOrderType switch
            {
                EOrderType.Default => songMasterTables.Select(x => x.Group).Distinct().ToList(),
                EOrderType.Level => songMasterTables.OrderBy(x => x.Difficulty == selectedDifficulty).Select(x => x.Group).Distinct().ToList(),
                EOrderType.HighScore => songMasterTables.OrderBy(_ => scoreManager.ScoreDataList.AsValueEnumerable().OrderBy(x => x.ScoreNum).ToList()).Select(x => x.Group).Distinct().ToList(),
                EOrderType.Name => songMasterTables.OrderBy(x => x.Name).Select(x => x.Group).Distinct().ToList(),
                _ => default
            };
        }

        public void SetNextOrderType() => CurrentOrderType = FastEnum.GetValues<EOrderType>().AsValueEnumerable().ElementAtOrDefault((int)CurrentOrderType + 1);

        public void SaveOrderType() => PlayerPrefsValues.Set(EKey.OrderType, CurrentOrderType.GetLength());
    }
}
