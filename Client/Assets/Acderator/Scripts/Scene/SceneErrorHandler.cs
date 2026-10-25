using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public sealed class SceneErrorHandler : IDisposable
{
    private Func<UniTask> onError;

    public void Register(Func<UniTask> onError)
    {
        this.onError = onError;
        Application.logMessageReceived -= OnLogMessageReceived;
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Exception) return;
        onError().Forget();
    }

    public void Dispose() => Application.logMessageReceived -= OnLogMessageReceived;
}