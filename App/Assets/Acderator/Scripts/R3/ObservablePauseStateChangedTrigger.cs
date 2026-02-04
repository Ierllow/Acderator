#if UNITY_EDITOR
using UnityEditor;

namespace R3.Triggers
{
    public class ObservablePauseStateChangedTrigger : ObservableTriggerBase
    {
        private Subject<PauseState> pauseStateChanged;

        private void Start() => EditorApplication.pauseStateChanged += PauseStateChanged;

        private void PauseStateChanged(PauseState state) => pauseStateChanged?.OnNext(state);

        public Observable<PauseState> OnPauseStateChangedAsObservable() => pauseStateChanged ??= new Subject<PauseState>();

        protected override void RaiseOnCompletedOnDestroy()
        {
            EditorApplication.pauseStateChanged -= PauseStateChanged;
            pauseStateChanged?.OnCompleted();
        }
    }
}
#endif