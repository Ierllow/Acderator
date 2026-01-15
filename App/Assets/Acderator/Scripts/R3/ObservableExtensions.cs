using System;
using System.Threading;
using System.Threading.Tasks;

namespace R3
{
    public static partial class ObservableExtensions
    {
        private static readonly TimeSpan throttleMilliSecs = TimeSpan.FromMilliseconds(600);

        public static IDisposable SubscribeLockAwait<T>(this Observable<T> source, ReactiveProperty<bool> gate, Func<T, CancellationToken, ValueTask> onNextAsync)
            => source.ThrottleFirst(throttleMilliSecs).Select((gate, onNextAsync), static (arg, param) => (arg, param.gate, param.onNextAsync)).Where(static param => param.gate.CurrentValue).SubscribeAwait(static async (param, ct) =>
        {
            param.gate.Value = false;
            try
            {
                await param.onNextAsync(param.arg, ct);
            }
            finally
            {
                param.gate.Value = true;
            }
        }, AwaitOperation.Drop);

        public static IDisposable SubscribeLock<T>(this Observable<T> source, ReactiveProperty<bool> gate, Action<T> onNext)
            => source.ThrottleFirst(throttleMilliSecs).Select((gate, onNext), static (arg, param) => (arg, param.gate, param.onNext)).Where(static param => param.gate.CurrentValue).Subscribe(static param =>
        {
            param.gate.Value = false;
            try
            {
                param.onNext(param.arg);
            }
            finally
            {
                param.gate.Value = true;
            }
        });
    }
}