using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense.Data;
using Intense.Master;
using System.Collections.Generic;
using ZLinq;

namespace SongSelect
{
    public class SongSelectCellListController
    {
        public int SelectedGroup { get; private set; } = LocalDataManager.Instance.SelectSavaData.SelectedGroup;
        public int SelectedDifficulty { get; private set; } = LocalDataManager.Instance.SelectSavaData.SelectedDifficulty;
        public List<int> SongGroupList { get; private set; } = MasterDataManager.Instance.MemoryDatabase.SongMasterTable.Select(x => x.Group).Distinct().ToList();

        public IUniTaskAsyncEnumerable<SongSelectCell> EverySelectedCellChangedAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.selectedCell).Queue().Where(x => x != null);
        public int SelectedCellListSid => MasterDataManager.Instance.MemoryDatabase.SongMasterTable.First(x => x.Group == SelectedGroup && x.Difficulty == SelectedDifficulty).Sid;

        public int IndexOf => SongGroupList.IndexOf(SelectedGroup);

        private SongSelectCell selectedCell;

        public void UpdateSelectedCell(SongSelectCell selectedCell) => this.selectedCell = selectedCell;

        public void UpdateSongGroupList(List<int> songGroupList) => SongGroupList = songGroupList;

        public void UpdateSelectedDifficulty(int selectedDifficulty) => SelectedDifficulty = selectedDifficulty;

        public void ChangeSelectedCell(SongSelectCell cell)
        {
            SelectedGroup = cell.MSong.Group;
            selectedCell.SetSelectedCell(false);
            cell.SetSelectedCell(true);
            selectedCell = cell;
        }

        public void SaveSelectedCell()
        {
            LocalDataManager.Instance.SelectSavaData.SelectedGroup = SelectedGroup;
            LocalDataManager.Instance.SelectSavaData.SelectedDifficulty = SelectedDifficulty;
        }
    }
}