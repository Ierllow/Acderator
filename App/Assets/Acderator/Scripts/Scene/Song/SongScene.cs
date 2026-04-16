using Cysharp.Threading.Tasks;
using Intense;
using Intense.Api;
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

        [InjectOptional] private SongApplicationPauseHandler songApplicationPauseHandler;
        [Inject] private NotesManager notesManager;
        [Inject] private NoteFactory noteFactory;
        [Inject] private SongControllerResolver songControllerResolver;
        [Inject] private SongSceneContext sceneContext;
        [Inject] private SoundManager soundManager;
        [Inject] private NetworkManager networkManager;
        [Inject] private SongAssetLoader songAssetLoader;

        protected override void Awake()
        {
            failFastExceptionWatcher.Init(destroyCancellationToken);
            base.Awake();
        }

        protected override void Start()
        {
            StartSubscribes();
            base.Start();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (songApplicationPauseHandler?.IsHandlePause(pauseStatus) == false) return;
            songPopupLayerController.OnOpenPausePopup(!sceneContext.IsAuto);
        }

        public override void OnCreateScene() => UniTask.Void(async () =>
        {
            if (!sceneContext.IsRestart)
            {
                await songAssetLoader.LoadBundles(sceneManager.CurrentSceneType, destroyCancellationToken).AddWatcherTo(failFastExceptionWatcher);
            }
            await (sceneContext.IsRestart ? sceneManager.FadeInAsync().AddWatcherTo(failFastExceptionWatcher) : UniTask.WhenAll(backTelopLayerController.ShowSongIntro(sceneContext.SongInfo)), sceneManager.FadeInAsync());
            songLayerController.Show(sceneContext.IsAuto);
            await notesLineController.Show().AddWatcherTo(failFastExceptionWatcher);
            backTelopLayerController.SetBackgroundImage(sceneContext.SongInfo.Bg);
            songControllerResolver.Loop.UpdateState(ESongState.Ready);
        });

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