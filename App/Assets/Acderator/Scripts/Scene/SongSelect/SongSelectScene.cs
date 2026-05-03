using Cysharp.Threading.Tasks;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Attribute;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using System.Linq;
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
        [Inject] private MasterDataManager masterDataManager;
        [Inject] private SoundManager soundManager;
        [Inject] private AssetBundleManager assetBundleManager;
        [Inject] private NetworkManager networkManager;
        [Inject] private ScoreManager scoreManager;

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
            backgroundImage.SetAtlasFormat("{0}", masterDataManager.MemoryDatabase.SongMasterTable.FindByGroup(songListView.SelectedGroup).Bg, "songselect/bg");
            orderButton.SetButtonText(songListView.CurrentOrderText);
            soundManager.PlaySongPreview(songListView.SelectedGroup, destroyCancellationToken);
            await UniTask.WhenAll(
                UniTask.Delay(500, cancellationToken: destroyCancellationToken),
                sceneManager.FadeInAsync());
        });

        public override void OnDeleteScene()
        {
            songListView.Save();
            base.OnDeleteScene();
        }

        private async UniTask LoadAssets()
        {
            var groupList = masterDataManager.MemoryDatabase.SongMasterTable.Select(x => x.Group).ToList();
            sceneContext.SongSelectBundleNameList(groupList).ForEach(assetBundleManager.AddLoadAssets);
            await assetBundleManager.LoadAssetsAsync(sceneManager.CurrentSceneType, destroyCancellationToken);
        }

        private async UniTask TapDecideButton()
        {
            var request = new ScoreBeginRequest();
            request.PostData.Add("score_id", songListView.SelectedCellListSid);
            var response = await networkManager.RequestAsync(request) as ScoreBeginResponse;
            if (!(response?.IsSuccess ?? false)) return;

            var groupList = masterDataManager.MemoryDatabase.SongMasterTable.Select(x => x.Group).ToList();
            await sceneManager.ChangeSceneAsync(ESceneType.Song, sceneContext.ToSongSceneContext(masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(songListView.SelectedCellListSid), autoButton.IsOn, response.SessionId));
        }

        private void TapOrderButton()
        {
            songListView.ReloadList();
            orderButton.SetButtonText(songListView.CurrentOrderText);
        }

        private void SelectedCellChanged(SongSelectCell cell)
        {
            songSelectDetail.SetData(cell.MSong.Group, songListView.SelectedDifficulty);
            backgroundImage.SetAtlasFormat("{0}", masterDataManager.MemoryDatabase.SongMasterTable.FindByGroup(cell.MSong.Group).Bg);
            soundManager.PlaySongPreview(cell.MSong.Group, destroyCancellationToken);
        }

        private void SelectedDifficultChanged(Toggle toggle)
        {
            songListView.UpdateSelectedDifficulty(int.Parse(toggle.name));
            var persent = scoreManager.GetScore(songListView.SelectedCellListSid) % masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(songListView.SelectedCellListSid).Score / 10000;
            songSelectDetail.UpdateInfo(songListView.SelectedDifficulty, scoreManager.GetScore(songListView.SelectedCellListSid), persent);
        }
    }
}
