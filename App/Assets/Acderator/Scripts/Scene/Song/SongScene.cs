using Cysharp.Threading.Tasks;
using Intense;
using Intense.Attribute;
using Intense.UI;
using UnityEngine;
using Zenject;

namespace Song
{
    [SceneType(ESceneType.Song)]
    public partial class SongScene : SceneBase
    {
        [RequiredField, SerializeField] private SongLayerController songLayerController;
        [RequiredField, SerializeField] private FrontTelopLayerController frontTelopLayerController;
        [RequiredField, SerializeField] private BackTelopLayerController backTelopLayerController;
        [RequiredField, SerializeField] private SongPopupLayerController songPopupLayerController;
        [RequiredField, SerializeField] private NotesLineController notesLineController;
        [RequiredField, SerializeField] private FailFastExceptionWatcher failFastExceptionWatcher;

        [Inject] private SongControllerResolver songControllerResolver;
        [Inject] private SongManagerResolver songManagerResolver;
        [Inject] private SongSceneContext sceneContext;
        [Inject] private SongAssetLoader songAssetLoader;

        protected override void Awake()
        {
            failFastExceptionWatcher.Init(destroyCancellationToken);
            base.Awake();
        }

        private void Start() => StartSubscribes();

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) return;
            if (sceneContext.SongMode != ESongMode.Normal) return;
            if (songManagerResolver.Sound.SongExPlayer.IsPlayEnd() || songManagerResolver.Notes.AliveNoteList.Count == 0) return;

            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto);
        }

        public override void OnCreateScene() => UniTask.Void(async () => await LoadAssets());

        protected override async UniTask OnErrorScene()
        {
            songControllerResolver.Loop.UpdateState(ESongState.Stop);
            if (!sceneManager.IsFadeIn) await sceneManager.FadeInAsync();
            await songPopupLayerController.DetectedError(new AlertError());
        }

        private async UniTask ChangeStateNext(ESongState songState)
        {
            switch (songState)
            {
                case ESongState.Ready: await OnReadySong(); break;
                case ESongState.Playing: OnPlayingSong(); break;
                case ESongState.Stop: OnStopSong(); break;
                case ESongState.End: await OnEndSong(); break;
                default: break;
            }
        }
    }
}