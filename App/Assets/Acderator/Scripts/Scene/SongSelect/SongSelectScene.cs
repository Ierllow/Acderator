using Cysharp.Threading.Tasks;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Master;
using Intense.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SongSelect
{
    [SceneType(ESceneType.SongSelect)]
    public partial class SongSelectScene : SceneBase
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
            StartSubscribes();
            base.Start();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) songListView.Save();
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

        private async UniTask TapDecideButton()
        {
            var request = new ScoreBeginRequest();
            request.PostData.Add("sid", songListView.SelectedCellListSid);
            var response = await NetworkManager.Instance.RequestAsync(request) as ScoreBeginResponse;
            await SceneManager.Instance.ChangeSceneAsync(ESceneType.Song, sceneContext.ToSongSceneContext(songListView.SelectedCellListSid, autoButton.IsOn, response.SessionId));
        }

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