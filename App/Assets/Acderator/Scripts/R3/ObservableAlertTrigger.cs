using System;
using UnityEngine;
using static UnityEngine.Application;

namespace R3.Triggers
{
    public class ObservableAlertTrigger : ObservableTriggerBase
    {
        private LogCallback logMessageReceived;

        private Subject<bool> onAlert;

        private void Awake()
        {
            logMessageReceived = (_, __, type) => onAlert?.OnNext(type.EnumEquals(LogType.Exception));
            Application.logMessageReceived += logMessageReceived;
        }

        public Observable<bool> OnAlertAsObservable()
        {
            return onAlert ??= new Subject<bool>();
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            Application.logMessageReceived -= logMessageReceived;
            onAlert?.OnCompleted();
        }
    }
}