using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Master;
using R3;
using R3.Triggers;
using System;
using UnityEditor;

namespace Song
{
    public partial class SongScene
    {
        private void StartSubscribes()
        {
            StartSongLogicSubscribesCore();
            StartFingerSubscribesCore();
            StartLayerSubscribesCore();
            StartPauseSubscribesCore();
        }

        private void StartSongLogicSubscribesCore()
        {
            songControllerResolver.Loop.EveryUpdateSongStateWhere.Subscribe(_ => UpdateSongProgressNext()).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.EverySongStateChanged.SubscribeAwait(ChangeStateNext).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.SongLoopUpdateSubject.Subscribe(OnSongLoop).RegisterTo(destroyCancellationToken);
            songControllerResolver.Spawner.NoteFactorySubject.Where(x => x != default).Subscribe(x => notesManager.AddAliveNote(noteFactory.SpawnNote(x))).RegisterTo(destroyCancellationToken);
            songControllerResolver.Loop.SongSecondsZeroWhere.Skip(1).Subscribe(_ => songControllerResolver.Loop.UpdateState(ESongState.Playing)).RegisterTo(destroyCancellationToken);
        }

        private void StartFingerSubscribesCore()
        {
            songControllerResolver.Auto?.FingerInfoSubject.Subscribe(x => FingerSubscribeNext(x, -1)).RegisterTo(destroyCancellationToken);
            songControllerResolver.Finger?.EveryUseTouchChanged.Subscribe(notesLineController.SetLaneLightActiveAll).RegisterTo(destroyCancellationToken);
            songControllerResolver.Finger?.FingerInfoSubject.TakeWhile(_ => !sceneContext.IsAuto).Subscribe((x) => FingerSubscribeNext(x.Item1, x.Item2)).RegisterTo(destroyCancellationToken);
        }

        private void StartLayerSubscribesCore()
        {
            songPopupLayerController.ClosedPausePopupAsAsyncEnumerable.TakeWhile(_ => sceneContext.SongMode.EnumEquals(ESongMode.Normal)).SubscribeAwait(ClosedPausePopupSubscribeNext).RegisterTo(destroyCancellationToken);
            songPopupLayerController.EverySceneTypeChanged.Where(s => !(s.EnumEquals(ESceneType.None) && songControllerResolver.Loop.SongStateEnumEquals(ESongState.End)) && s.EnumEquals(ESceneType.Result)).SubscribeAwait(async (s, _) => await SceneManager.Instance.ChangeSceneAsync(s, sceneContext.ToResultSceneContext(songLayerController.CurrentScore, songLayerController.JudgeCountDict))).RegisterTo(destroyCancellationToken);
            songLayerController.OnTapPauseButtonAsObservable.TakeWhile(_ => sceneContext.SongMode.EnumEquals(ESongMode.Normal) && !songControllerResolver.Loop.SongStateEnumEquals(ESongState.End)).Subscribe(_ => { songControllerResolver.Loop.UpdateState(ESongState.Stop); ApplicationPause(false); }).RegisterTo(destroyCancellationToken);
            songLayerController.Subscribes(_ => !sceneContext.IsAuto, destroyCancellationToken);
            songControllerResolver.TutorialState?.TutorialEventSubject.Subscribe(songControllerResolver.Tutorial.UpdateTutorial).RegisterTo(destroyCancellationToken);
        }

        private void StartPauseSubscribesCore()
        {
            this.OnApplicationPauseAsObservable().Where(_ => sceneContext.SongMode.EnumEquals(ESongMode.Normal)).TakeWhile(_ => !SoundManager.Instance.SongExPlayer.IsPlayEnd() && notesManager.AliveNoteList.Count > 0).Subscribe(ApplicationPause).RegisterTo(destroyCancellationToken);
#if UNITY_EDITOR
            this.OnPauseStateChangedAsObservable().Subscribe((state) =>
            {
                if (state.EnumEquals(PauseState.Paused))
                {
                    songControllerResolver.Loop.UpdateState(ESongState.Stop);
                    songPopupLayerController.OnOpenPausePopup(sceneContext.IsAuto);
                    return;
                }
                songControllerResolver.Loop.UpdateState(ESongState.Playing);
            }).RegisterTo(destroyCancellationToken);
#endif
        }
    }
}