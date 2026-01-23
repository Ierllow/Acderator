using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Asset;
using Intense.Master;
using Intense.UI;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using ZLinq;

namespace SongSelect
{
    public class SongSelectScene : SceneBase
    {
        [SerializeField] private AtlasImage backgroundImage;
        [SerializeField] private CommonButton decideButton;
        [SerializeField] private CommonButton orderButton;
        [SerializeField] private ToggleEx autoButton;
        [SerializeField] private SongListView songListView;
        [SerializeField] private SongSelectDetail songSelectDetail;

        [Inject] private SongSelectSceneContext sceneContext;

        protected override void Start()
        {
            autoButton.SetToggleTextAsAsyncEnumerableForEachAsync(isOn => isOn ? "オートON" : "オートOFF");
            decideButton.OnTapButtonAsObservable.SubscribeLockAwait(new(true), async (_, __) => await TapDecideButton()).RegisterTo(destroyCancellationToken);
            orderButton.OnTapButtonAsObservable.SubscribeLock(new(true), _ => TapOrderButton()).RegisterTo(destroyCancellationToken);
            songListView.EverySelectedCellChanged.Skip(1).Subscribe(SelectedCellChanged).RegisterTo(destroyCancellationToken);
            songSelectDetail.EveryToggleChanged.Skip(1).Where(x => x != null).Subscribe(SelectedDifficultChanged).RegisterTo(destroyCancellationToken);
            this.OnApplicationPauseAsObservable().Where(x => x).Subscribe(_ => songListView.Save()).RegisterTo(destroyCancellationToken);
            base.Start();
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            await LoadAssets();
            songListView.Init();
            songSelectDetail.SetData(songListView.SelectedGroup, songListView.SelectedDifficulty);
            backgroundImage.SetAtlasFormat("{0}", MasterDataManager.Instance.MemoryDatabase.SongMasterTable.FindByGroup(songListView.SelectedGroup).Bg, "songselect/bg");
            orderButton.SetButtonText(songListView.CurrentOrderText);
            SoundManager.Instance.PlaySongPreview(songListView.SelectedGroup, destroyCancellationToken);
            await UniTask.WhenAll(
                UniTask.Delay(500, cancellationToken: destroyCancellationToken),
                SceneManager.Instance.FadeInAsync());
        });

        public override void OnDeleteScene()
        {
            songListView.Save();
            base.OnDeleteScene();
        }

        private async UniTask LoadAssets()
        {
            sceneContext.SongSelectBundleNameList.ForEach(AssetBundleManager.Instance.AddLoadAssets);
            await AssetBundleManager.Instance.LoadAssetsAsync(destroyCancellationToken);
        }

        private async UniTask TapDecideButton() => await SceneManager.Instance.ChangeSceneAsync(ESceneType.Song, sceneContext.ToSongSceneContext(songListView.SelectedCellListSid, autoButton.IsOn));

        private void TapOrderButton()
        {
            songListView.ReloadList();
            orderButton.SetButtonText(songListView.CurrentOrderText);
        }

        private void SelectedCellChanged(SongSelectCell cell)
        {
            songSelectDetail.SetData(cell.MSong.Group, songListView.SelectedDifficulty);
            backgroundImage.SetAtlasFormat("{0}", MasterDataManager.Instance.MemoryDatabase.SongMasterTable.FindByGroup(cell.MSong.Group).Bg);
            SoundManager.Instance.PlaySongPreview(cell.MSong.Group, destroyCancellationToken);
        }

        private void SelectedDifficultChanged(Toggle toggle)
        {
            songListView.UpdateSelectedDifficulty(int.Parse(toggle.name));
            songSelectDetail.UpdateInfo(songListView.SelectedDifficulty);
        }
    }
}