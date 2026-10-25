using System;
using System.Threading;
using System.Threading.Tasks;

namespace R3
{
    public static partial class ObservableExtensions
    {
        private static readonly TimeSpan throttleMilliSecs = TimeSpan.FromMilliseconds(600);

        public static IDisposable SubscribeLockAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync)
        {
            var canSubscribe = true;
            return source.ThrottleFirst(throttleMilliSecs).Where(_ => canSubscribe).SubscribeAwait(async (value, ct) =>
            {
                canSubscribe = false;
                try
                {
                    await onNextAsync(value, ct);
                }
                finally
                {
                    canSubscribe = true;
                }
            }, AwaitOperation.Drop);
        }

        public static IDisposable SubscribeLock<T>(this Observable<T> source, Action<T> onNext)
        {
            var canSubscribe = true;
            return source.ThrottleFirst(throttleMilliSecs).Where(_ => canSubscribe).Subscribe(value =>
            {
                canSubscribe = false;
                try
                {
                    onNext(value);
                }
                finally
                {
                    canSubscribe = true;
                }
            });
        }
    }
}