using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Api;
using Intense.Asset;
using Intense.Attribute;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using R3;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SongSelect
{
    [SceneType(SceneType.SongSelect)]
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
        [Inject] private readonly AddressableAssetManager addressableAssetManager;
        [Inject] private readonly NetworkManager networkManager;
        [Inject] private readonly ScoreManager scoreManager;
        [Inject] private readonly SongSelectSoundController soundController;
        [Inject] private readonly Song.TutorialSceneContextBuilder tutorialSceneContextBuilder;

        private void Start()
        {
            autoButton.SetToggleTextAsAsyncEnumerableForEachAsync(isOn => isOn ? "オートON" : "オートOFF");
            decideButton.OnTapButtonAsObservable.SubscribeLockAwait(async (_, __) => await TapDecideButton()).RegisterTo(destroyCancellationToken);
            orderButton.OnTapButtonAsObservable.SubscribeLock(_ => TapOrderButton()).RegisterTo(destroyCancellationToken);
            songListView.EverySelectedCellChanged.Skip(1).Subscribe(SelectedCellChanged).RegisterTo(destroyCancellationToken);
            songSelectDetail.EveryToggleChanged.Skip(1).Where(x => x != null).Subscribe(SelectedDifficultChanged).RegisterTo(destroyCancellationToken);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) songListView.Save();
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            await LoadAssets();
            songListView.Init();
            var selectedSong = masterDataManager.MemoryDatabase.SongMasterTable.FindByGroup(songListView.SelectedGroup);
            songSelectDetail.SetData(songListView.SelectedGroup, songListView.SelectedDifficulty, addressableAssetManager.GetSprite(songListView.SelectedGroup.ToString()));
            backgroundImage.SetSprite(addressableAssetManager.GetSprite(selectedSong.Bg.ToString()));
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
            foreach (var address in sceneContext.DynamicAssetAddressList(groupList))
            {
                addressableAssetManager.AddLoad(address);
            }
            await addressableAssetManager.LoadAssetsAsync(sceneManager.CurrentSceneType, destroyCancellationToken);
        }

        private async UniTask TapDecideButton()
        {
            var selectedSong = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(songListView.SelectedCellListSid);
            var request = new ScoreBeginRequest { ScoreId = songListView.SelectedCellListSid };
            var response = await networkManager.RequestAsync(request);
            if (!(response?.IsSuccess ?? false)) return;

            if (!PlayerPrefsValues.IsTutorialCompleted && !autoButton.IsOn && tutorialSceneContextBuilder.TryBuild(selectedSong, response.SessionId, out var tutorialSceneContext))
            {
                await sceneManager.ChangeSceneAsync(SceneType.Song, tutorialSceneContext);
                return;
            }

            await sceneManager.ChangeSceneAsync(SceneType.Song, sceneContext.ToSongSceneContext(selectedSong, autoButton.IsOn, response.SessionId));
        }

        private void TapOrderButton()
        {
            songListView.ReloadList();
            orderButton.SetButtonText(songListView.CurrentOrderText);
        }

        private void SelectedCellChanged(SongSelectCell cell)
        {
            songSelectDetail.SetData(cell.Song.Group, songListView.SelectedDifficulty, addressableAssetManager.GetSprite(cell.Song.Group.ToString()));
            backgroundImage.SetSprite(addressableAssetManager.GetSprite(cell.Song.Bg.ToString()));
            soundController.PlayPreview(cell.Song.Group, destroyCancellationToken);
        }

        private void SelectedDifficultChanged(Toggle toggle)
        {
            songListView.UpdateSelectedDifficulty(int.Parse(toggle.name));
            var sid = songListView.SelectedCellListSid;
            var score = scoreManager.GetScore(sid);
            var percent = score * 100f / masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sid).Score;
            var rank = (int)ScoreUtils.ToRank(score, true);
            songSelectDetail.UpdateInfo(
                score,
                percent,
                addressableAssetManager.GetSprite(string.Format("icon_result_rank_{0}", rank)));
        }
    }
}