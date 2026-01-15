using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace R3.Triggers
{
    public static partial class ObservableTriggerExtensions
    {
        public static Observable<bool> OnApplicationPauseAsObservable(this GameObject gameObject) => gameObject == null ? Observable.Empty<bool>() : gameObject.GetOrAddComponent<ObservableOnApplicationPauseTrigger>().OnApplicationPauseAsObservable();
        public static Observable<(string, Action<SpriteAtlas>)> OnAtlasRequestedAsObservable(this GameObject gameObject) => gameObject == null ? Observable.Empty<(string, Action<SpriteAtlas>)>() : gameObject.GetOrAddComponent<ObservableAtlasRequestedTrigger>().OnAtlasRequestedAsObservable();
        public static Observable<PointerType> OnPointerUpDownAsObservable(this GameObject gameObject) => gameObject == null ? Observable.Empty<PointerType>() : gameObject.GetOrAddComponent<ObservableOnPointerUpDownTrigger>().OnPointerUpDownAsObservable();
        public static Observable<bool> OnAlertAsObservable(this GameObject gameObject) => gameObject == null ? Observable.Empty<bool>() : gameObject.GetOrAddComponent<ObservableAlertTrigger>().OnAlertAsObservable();
#if UNITY_EDITOR
        public static Observable<PauseState> OnPauseStateChangedAsObservable(this GameObject gameObject) => gameObject == null ? Observable.Empty<PauseState>() : gameObject.GetOrAddComponent<ObservablePauseStateChangedTrigger>().OnPauseStateChangedAsObservable();
#endif
        public static Observable<bool> OnApplicationPauseAsObservable(this Component component) => OnApplicationPauseAsObservable(component.gameObject);
        public static Observable<(string, Action<SpriteAtlas>)> OnAtlasRequestedAsObservable(this Component component) => OnAtlasRequestedAsObservable(component.gameObject);
        public static Observable<PointerType> OnPointerUpDownAsObservable(this Component component) => OnPointerUpDownAsObservable(component.gameObject);
        public static Observable<bool> OnAlertAsObservable(this Component gameObject) => OnAlertAsObservable(gameObject.gameObject);
#if UNITY_EDITOR
        public static Observable<PauseState> OnPauseStateChangedAsObservable(this Component gameObject) => OnPauseStateChangedAsObservable(gameObject.gameObject);
#endif

        private static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
    }
}