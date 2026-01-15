using UnityEngine;
using UnityEngine.EventSystems;

namespace R3.Triggers
{
    public enum PointerType { None, Up, Down }

    [DisallowMultipleComponent]
    public class ObservableOnPointerUpDownTrigger : ObservableTriggerBase, IPointerUpHandler, IPointerDownHandler
    {
        private Subject<PointerType> onPointerUpDown;

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => onPointerUpDown?.OnNext(PointerType.Up);

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => onPointerUpDown?.OnNext(PointerType.Down);

        public Observable<PointerType> OnPointerUpDownAsObservable() => onPointerUpDown ??= new Subject<PointerType>();

        protected override void RaiseOnCompletedOnDestroy() => onPointerUpDown?.OnCompleted();
    }
}