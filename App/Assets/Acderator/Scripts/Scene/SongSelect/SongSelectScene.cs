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

        [Inject] private readonly SongSelectSceneContext sceneContext;
        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly AssetBundleManager assetBundleManager;
        [Inject] private readonly NetworkManager networkManager;
        [Inject] private readonly ScoreManager scoreManager;
        [Inject] private readonly SongSelectSoundController soundController;
        [Inject] private readonly Song.TutorialSceneContextBuilder tutorialSceneContextBuilder;

        private void Start() => StartSubscribes();

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
            soundController.PlayPreview(songListView.SelectedGroup, destroyCancellationToken);
            await UniTask.WhenAll(
                UniTask.Delay(500, cancellationToken: destroyCancellationToken),
                sceneManager.FadeInAsync());
        });

        public override void OnDeleteScene()
        {
            soundController.StopPreview();
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
            var selectedSong = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(songListView.SelectedCellListSid);
            var request = new ScoreBeginRequest { ScoreId = songListView.SelectedCellListSid };
            var response = await networkManager.RequestAsync(request);
            if (!(response?.IsSuccess ?? false)) return;

            if (!PlayerPrefsValues.IsTutorialCompleted && !autoButton.IsOn && tutorialSceneContextBuilder.TryBuild(selectedSong, response.SessionId, out var tutorialSceneContext))
            {
                await sceneManager.ChangeSceneAsync(ESceneType.Song, tutorialSceneContext);
                return;
            }

            await sceneManager.ChangeSceneAsync(ESceneType.Song, sceneContext.ToSongSceneContext(selectedSong, autoButton.IsOn, response.SessionId));
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
            soundController.PlayPreview(cell.MSong.Group, destroyCancellationToken);
        }

        private void SelectedDifficultChanged(Toggle toggle)
        {
            songListView.UpdateSelectedDifficulty(int.Parse(toggle.name));
            var persent = scoreManager.GetScore(songListView.SelectedCellListSid) % masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(songListView.SelectedCellListSid).Score / 10000;
            songSelectDetail.UpdateInfo(songListView.SelectedDifficulty, scoreManager.GetScore(songListView.SelectedCellListSid), persent);
        }
    }
}