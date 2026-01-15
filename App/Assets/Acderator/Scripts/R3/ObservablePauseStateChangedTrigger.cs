#if UNITY_EDITOR
using UnityEditor;

namespace R3.Triggers
{
    public class ObservablePauseStateChangedTrigger : ObservableTriggerBase
    {
        private Subject<PauseState> puaseStateChanged;

        private void Start() => EditorApplication.pauseStateChanged += PuaseStateChanged;

        private void PuaseStateChanged(PauseState state) => puaseStateChanged?.OnNext(state);

        public Observable<PauseState> OnPauseStateChangedAsObservable() => puaseStateChanged ??= new Subject<PauseState>();

        protected override void RaiseOnCompletedOnDestroy()
        {
            EditorApplication.pauseStateChanged -= PuaseStateChanged;
            puaseStateChanged?.OnCompleted();
        }
    }
}
#endif