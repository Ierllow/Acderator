#nullable enable

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using R3;
using Zenject;

namespace Song
{
    public class SongLoopController
    {
        [Inject] private readonly SongTimeCalculator timeCalculator = default!;

        private readonly Subject<float> songLoopUpdateSubject = new();

        public SongState CurrentState { get; private set; }

        public IUniTaskAsyncEnumerable<SongState> EverySongStateChanged => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.CurrentState).Queue();
        public IUniTaskAsyncEnumerable<AsyncUnit> EveryUpdateSongStateWhere => UniTaskAsyncEnumerable.EveryUpdate().Queue().TakeWhile(_ => CurrentState != SongState.End);

        public Observable<float> SongLoopUpdateAsObservable => songLoopUpdateSubject;
        public Observable<float> SongLeadInCompletedAsObservable => songLoopUpdateSubject.Where(s => s >= 0).Take(1);

        public void StartLeadIn(float leadInSec) => timeCalculator.StartLeadIn(leadInSec);

        public void UpdateState(SongState currentState) => CurrentState = currentState;

        public void Tick(float sec) => songLoopUpdateSubject.OnNext(timeCalculator.GetSec(sec, CurrentState));
    }
}