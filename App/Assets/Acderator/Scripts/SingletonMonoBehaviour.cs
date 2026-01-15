using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _Instance;
    public static T Instance => _Instance ??= FindFirstObjectByType<T>();

    #region MonoBehaviour Handler
    protected virtual void Awake() => _Instance ??= this as T;
    protected virtual void OnDestroy() => _Instance = default;
    #endregion
}