using UnityEngine;

namespace R3.Triggers
{
    [DisallowMultipleComponent]
    public class ObservableOnApplicationPauseTrigger : ObservableTriggerBase
    {
        private Subject<bool> onApplicationPause;

        private void OnApplicationPause(bool pause) => onApplicationPause?.OnNext(pause);

        public Observable<bool> OnApplicationPauseAsObservable() => onApplicationPause ??= new Subject<bool>();

        protected override void RaiseOnCompletedOnDestroy() => onApplicationPause?.OnCompleted();
    }
}