using FastEnumUtility;
using Intense.Data;
using Intense.Master;
using System;
using System.Collections.Generic;
using ZLinq;

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

        public EOrderType CurrentOrderType { get; private set; } = (EOrderType)LocalDataManager.Instance.SongSelectSort.OrderType;

        public string Text => CurrentOrderType.GetTextAttribute().Text;

        public List<int> GetOrderedList(int selectedDifficulty)
        {
            var songMasterTables = MasterDataManager.Instance.MemoryDatabase.SongMasterTable;
            return CurrentOrderType switch
            {
                EOrderType.Default => songMasterTables.Select(x => x.Group).Distinct().ToList(),
                EOrderType.Level => songMasterTables.OrderBy(x => x.Difficulty == selectedDifficulty).Select(x => x.Group).Distinct().ToList(),
                EOrderType.HighScore => songMasterTables.OrderBy(_ => ScoreManager.Instance.ScoreDataList.AsValueEnumerable().OrderBy(x => x.ScoreNum).ToList()).Select(x => x.Group).Distinct().ToList(),
                EOrderType.Name => songMasterTables.OrderBy(x => x.Name).Select(x => x.Group).Distinct().ToList(),
                _ => default
            };
        }

        public void SetNextOrderType() => CurrentOrderType = FastEnum.GetValues<EOrderType>().AsValueEnumerable().ElementAtOrDefault((int)CurrentOrderType + 1);

        public void SaveOrderType() => LocalDataManager.Instance.SongSelectSort.OrderType = CurrentOrderType.GetLength();
    }
}