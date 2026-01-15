using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Intense.UI
{
    public sealed class FailFastExceptionWatcher : MonoBehaviour
    {
        private CancellationTokenSource cancellationTokenSource;

        public bool IsFailed { get; private set; }

        public void Init(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;
            Application.logMessageReceivedThreaded += OnLog;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandled;
            UniTaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void Fail(Exception _)
        {
            Cancel();
            IsFailed = true;
        }

        private void Cancel()
        {
            if (!cancellationTokenSource.IsCancellationRequested) cancellationTokenSource.Cancel();
        }

        private void OnLog(string _, string __, LogType type)
        {
            if (cancellationTokenSource.IsCancellationRequested) return;
            if (type.EnumEquals(LogType.Exception)) Cancel();
        }

        private void OnUnhandled(object sender, UnhandledExceptionEventArgs e)
        {
            if (!cancellationTokenSource.IsCancellationRequested && e.ExceptionObject is Exception ex) Fail(ex);
            else Cancel();
        }

        private void OnUnobservedTaskException(Exception e)
        {
            if (!cancellationTokenSource.IsCancellationRequested) Fail(e);
        }

        private void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= OnLog;
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandled;
            UniTaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

            cancellationTokenSource?.Dispose();
        }
    }

    public static class UniTaskWatcherExtensions
    {
        public static async UniTask AddWatcherTo(this UniTask task, FailFastExceptionWatcher watcher)
        {
            await task;
            if (watcher.IsFailed) throw new OperationCanceledException("FailFast Detected");
        }

        public static async UniTask<T> AddWatcherTo<T>(this UniTask<T> task, FailFastExceptionWatcher watcher)
        {
            var result = await task;
            return watcher.IsFailed ? throw new OperationCanceledException("FailFast Detected") : result;
        }
    }
}