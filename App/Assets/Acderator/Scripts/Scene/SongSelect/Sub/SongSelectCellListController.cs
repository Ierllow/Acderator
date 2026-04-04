using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense.Master;
using System.Collections.Generic;
using Zenject;
using ZLinq;

namespace SongSelect
{
    public class SongSelectCellListController : IInitializable
    {
        [Inject] private MasterDataManager masterDataManager;

        public int SelectedGroup { get; private set; } = PlayerPrefsValues.SDG;
        public int SelectedDifficulty { get; private set; } = PlayerPrefsValues.SDD;
        public List<int> SongGroupList { get; private set; }

        public IUniTaskAsyncEnumerable<SongSelectCell> EverySelectedCellChangedAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.selectedCell).Queue().Where(x => x != null);
        public int SelectedCellListSid => masterDataManager.MemoryDatabase.SongMasterTable.First(x => x.Group == SelectedGroup && x.Difficulty == SelectedDifficulty).Sid;

        public int IndexOf => SongGroupList.IndexOf(SelectedGroup);

        private SongSelectCell selectedCell;

        public void Initialize() => SongGroupList = masterDataManager.MemoryDatabase.SongMasterTable.Select(x => x.Group).Distinct().ToList();

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
            PlayerPrefsValues.Set(EKey.SelectedGroup, SelectedGroup);
            PlayerPrefsValues.Set(EKey.SelectedDifficulty, SelectedDifficulty);
        }
    }
}