using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using System;
using UnityEngine;
using R3;
using Zenject;

namespace Song
{
    public class SongLoopController : IController
    {
        [Inject] private FrameRateController frameRateController;

        private ESongState cachedSongState;
        private float offset;

        public float PauseTime { get; private set; }

        public IUniTaskAsyncEnumerable<ESongState> EverySongStateChanged => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.cachedSongState).Queue();
        public IUniTaskAsyncEnumerable<AsyncUnit> EveryUpdateSongStateWhere => UniTaskAsyncEnumerable.EveryUpdate().Queue().TakeWhile(_ => !SongStateEnumEquals(ESongState.End)).Where(_ => frameRateController.ShouldUpdate());
        public readonly Subject<float> SongLoopUpdateSubject = new();
        public Observable<float> SongSecondsZeroWhere => SongLoopUpdateSubject.Where(s => s >= 0);

        public void SetOffset(float offset) => this.offset = offset;

        public bool SongStateEnumEquals(ESongState state) => cachedSongState.EnumEquals(state);

        public bool SongStateEnumEquals(ESongState state1, ESongState state2) => cachedSongState.EnumEquals(state1) || cachedSongState.EnumEquals(state2);

        public void UpdateState(ESongState currentState) => cachedSongState = currentState;

        public void Tick()
        {
            if (SongStateEnumEquals(ESongState.None, ESongState.Stop)) PauseTime += Time.deltaTime;

            var sec = SoundManager.Instance.SongExPlayer.GetTime().ToSeconds();
            var elapsedSec = Time.timeSinceLevelLoad - PauseTime - offset;
            SongLoopUpdateSubject.OnNext(sec <= 0 ? elapsedSec : sec);
        }
    }
}