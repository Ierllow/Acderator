using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense.Master;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace SongSelect
{
    public class SongSelectCellListController : IInitializable
    {
        [Inject] private readonly MasterDataManager masterDataManager;

        public int SelectedGroup { get; private set; } = PlayerPrefsValues.SelectedGroup;
        public int SelectedDifficulty { get; private set; } = PlayerPrefsValues.SelectedDifficulty;
        public List<int> SongGroupList => songGroupList ??= CreateSongGroupList();

        public IUniTaskAsyncEnumerable<SongSelectCell> EverySelectedCellChangedAsAsyncEnumerable => CreateSelectedCellChanges();
        public int SelectedCellListSid => selectedCellListSid ??= FindSelectedSongSid();

        public int IndexOf => SongGroupList.IndexOf(SelectedGroup);

        private SongSelectCell selectedCell;
        private int? selectedCellListSid;
        private List<int> songGroupList;

        public void Initialize() => songGroupList = CreateSongGroupList();

        public void UpdateSelectedCell(SongSelectCell selectedCell) => this.selectedCell = selectedCell;

        public void UpdateSongGroupList(List<int> songGroupList) => this.songGroupList = songGroupList;

        public void UpdateSelectedDifficulty(int selectedDifficulty)
        {
            SelectedDifficulty = selectedDifficulty;
            selectedCellListSid = null;
        }

        public void ChangeSelectedCell(SongSelectCell cell)
        {
            SelectedGroup = cell.Song.Group;
            selectedCellListSid = null;
            selectedCell.SetSelectedCell(false);
            cell.SetSelectedCell(true);
            selectedCell = cell;
        }

        public void SaveSelectedCell()
        {
            PlayerPrefsValues.Set(PlayerPrefsKey.SelectedGroup, SelectedGroup);
            PlayerPrefsValues.Set(PlayerPrefsKey.SelectedDifficulty, SelectedDifficulty);
        }

        private IUniTaskAsyncEnumerable<SongSelectCell> CreateSelectedCellChanges()
        {
            var selectedCellChanges = UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.selectedCell).Queue();
            return selectedCellChanges.Where(x => x != null);
        }

        private int FindSelectedSongSid()
        {
            var songs = masterDataManager.MemoryDatabase.SongMasterTable;
            return songs.First(x => x.Group == SelectedGroup && x.Difficulty == SelectedDifficulty).Sid;
        }

        private List<int> CreateSongGroupList() => masterDataManager.MemoryDatabase.SongMasterTable.Select(x => x.Group).Distinct().ToList();
    }
}