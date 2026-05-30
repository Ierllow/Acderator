using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using R3;
using Zenject;

namespace Song
{
    public class SongLoopController : IController
    {
        [Inject] private readonly SongTimeCalculator timeCalculator;

        private readonly Subject<float> songLoopUpdateSubject = new();

        public ESongState CurrentState { get; private set; }

        public IUniTaskAsyncEnumerable<ESongState> EverySongStateChanged => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.CurrentState).Queue();
        public IUniTaskAsyncEnumerable<AsyncUnit> EveryUpdateSongStateWhere => UniTaskAsyncEnumerable.EveryUpdate().Queue().TakeWhile(_ => CurrentState != ESongState.End);

        public Observable<float> SongLoopUpdateAsObservable => songLoopUpdateSubject;
        public Observable<float> SongSecondsZeroWhere => songLoopUpdateSubject.Where(s => s >= 0);

        public void SetOffset(float offset) => timeCalculator.SetOffset(offset);

        public void UpdateState(ESongState currentState) => CurrentState = currentState;

        public void Tick(float sec) => songLoopUpdateSubject.OnNext(timeCalculator.GetSec(sec, CurrentState));
    }
}