using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using R3;
using UnityEngine;
using Zenject;

namespace Song
{
    public class SongLoopController : IController
    {
        [Inject] private FrameRateController frameRateController;

        private float offset;

        public ESongState CurrentState { get; private set; }
        public float PauseTime { get; private set; }

        public IUniTaskAsyncEnumerable<ESongState> EverySongStateChanged => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.CurrentState).Queue();
        public IUniTaskAsyncEnumerable<AsyncUnit> EveryUpdateSongStateWhere => UniTaskAsyncEnumerable.EveryUpdate().Queue().TakeWhile(_ => CurrentState != ESongState.End).Where(_ => frameRateController.ShouldUpdate());
        public readonly Subject<float> SongLoopUpdateSubject = new();
        public Observable<float> SongSecondsZeroWhere => SongLoopUpdateSubject.Where(s => s >= 0);

        public void SetOffset(float offset) => this.offset = offset;

        public void UpdateState(ESongState currentState) => CurrentState = currentState;

        public void Tick(float sec)
        {
            if (CurrentState is ESongState.None or ESongState.Stop) PauseTime += Time.deltaTime;

            var elapsedSec = Time.timeSinceLevelLoad - PauseTime - offset;
            SongLoopUpdateSubject.OnNext(sec <= 0 ? elapsedSec : sec);
        }
    }
}