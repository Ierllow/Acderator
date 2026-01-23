using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using Intense.Internal;
using Intense.Master;
using UnityEngine;
using Zenject;
using ZLinq;

namespace SongSelect
{
    public class SongListView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller scroller;
        [SerializeField] private EnhancedScrollerCellView cellPrefab;

        [Inject] private SongSelectCellListController songSelectCellListController;
        [Inject] private SongSelectSortController songSelectSortController;

        public int SelectedGroup => songSelectCellListController.SelectedGroup;
        public int SelectedDifficulty => songSelectCellListController.SelectedDifficulty;
        public int SelectedCellListSid => songSelectCellListController.SelectedCellListSid;
        public string CurrentOrderText => songSelectSortController.Text;

        public IUniTaskAsyncEnumerable<SongSelectCell> EverySelectedCellChanged => songSelectCellListController.EverySelectedCellChangedAsAsyncEnumerable;

        public void Init()
        {
            scroller.Delegate ??= this;
            scroller.cellViewVisibilityChanged = (cellView) =>
            {
                var cell = (SongSelectCell)cellView;
                cell.SetSelectedCell(cell.MSong.Group == songSelectCellListController.SelectedGroup);
            };
            scroller.cellViewInstantiated = (_, cellView) =>
            {
                var songSelectCell = cellView as SongSelectCell;
                if (songSelectCell.MSong?.Group == songSelectCellListController.SelectedGroup)
                {
                    songSelectCellListController.UpdateSelectedCell(songSelectCell);
                    songSelectCell.SetSelectedCell(true);
                }
            };
            scroller.ReloadData();

            scroller.JumpToDataIndex(songSelectCellListController.IndexOf, loopJumpDirection: EnhancedScroller.LoopJumpDirectionEnum.Up);
        }

        public int GetNumberOfCells(EnhancedScroller _) => songSelectCellListController.SongGroupList.AsValueEnumerable().Count();

        public float GetCellViewSize(EnhancedScroller _, int __) => 100f;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller _, int dataIndex, int __)
        {
            var cellView = scroller.GetCellView(cellPrefab) as SongSelectCell;
            var group = songSelectCellListController.SongGroupList.AsValueEnumerable().ElementAt(dataIndex);
            cellView.Setup(MasterDataManager.Instance.MemoryDatabase.SongMasterTable.FindByGroup(group), songSelectCellListController.ChangeSelectedCell);
            return cellView;
        }

        public void ReloadList() => UniTask.Void(async () =>
        {
            songSelectSortController.SetNextOrderType();
            songSelectCellListController.UpdateSongGroupList(songSelectSortController.GetOrderedList(SelectedDifficulty));
            var dataIndexPosition = (int)(scroller.ScrollPosition / GetCellViewSize(default, 0));
            scroller.ReloadData(scroller.NormalizedScrollPosition);
            await UniTask.NextFrame();
            scroller.JumpToDataIndex(dataIndexPosition);
        });

        public void UpdateSelectedDifficulty(int selectedDifficulty) => songSelectCellListController.UpdateSelectedDifficulty(selectedDifficulty);

        public void Save()
        {
            Error.ThrowIfInvalid(SelectedGroup >= 0, "Invalid selected group");
            Error.ThrowIfInvalid(SelectedDifficulty >= 0, "Invalid selected difficulty");
            Error.ThrowIfInvalid(SelectedCellListSid >= 0, "Invalid selected cell list sid");
            songSelectCellListController.SaveSelectedCell();
            songSelectSortController.SaveOrderType();
        }
    }
}