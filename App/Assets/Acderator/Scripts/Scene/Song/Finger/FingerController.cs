#nullable enable

using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Song
{
    public enum EFingerType { None, Up, Down }

    public class FingerController : MonoBehaviour, IInitializable
    {
        [SerializeField] private Camera choose = default!;
        [SerializeField] private Transform hitPlane = default!;
        [SerializeField] private bool ignoreIsOverGui;
        [SerializeField] private bool ignoreStartedOverGui = true;
        [SerializeField] private float swipeThreshold = 50f;

        [Inject] private readonly NotesManager notesManager = default!;
        [Inject] private readonly NoteFactory noteFactory = default!;
        [Inject] private readonly FingerJudgeRequestController judgeRequestController = default!;
        [Inject] private readonly PointerInput pointerInput = default!;
        [Inject] private readonly LaneDetector laneDetector = default!;
        [Inject] private readonly TouchStateManager touchStateManager = default!;

        private bool useTouch;

        private readonly Subject<bool> useTouchSubject = new();

        public Observable<bool> EveryUseTouchChanged => useTouchSubject.Where(x => !x);

        private bool IsAuto => notesManager.SongOption.IsAuto;

        public void Initialize()
        {
            if (IsAuto) return;
            laneDetector.Init(choose, hitPlane, noteFactory.LaneParents);
            pointerInput.SetCallbacks(FingerDown, FingerUpdate, FingerUp);
            TrySetUseTouch(true);
        }

        public void UpdateInput()
        {
            if (IsAuto || !useTouch) return;
            pointerInput.Update();
        }

        public bool TrySetUseTouch(bool useTouch)
        {
            if (IsAuto || this.useTouch == useTouch) return false;

            this.useTouch = useTouch;
            if (!useTouch)
            {
                touchStateManager.Clear();
                judgeRequestController.Clear();
            }
            useTouchSubject.OnNext(useTouch);
            return true;
        }

        private void FingerDown(int pointerId, Vector2 screenPosition)
        {
            if ((ignoreStartedOverGui || ignoreIsOverGui) && IsPointerOverGui(pointerId)) return;
            if (!TryGetLane(screenPosition, out var lane)) return;

            touchStateManager.Set(pointerId, new TouchStateManager.TouchState(screenPosition, lane));

            judgeRequestController.Enqueue(FingerJudgeRequest.Down(lane));
        }

        private void FingerUpdate(int pointerId, Vector2 screenPosition)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return;
            if (ignoreIsOverGui && IsPointerOverGui(pointerId)) return;

            TryGetLane(screenPosition, out var lane);

            if (TryEnqueueHoldCross(pointerId, touchState.Lane, lane))
            {
                touchStateManager.Set(pointerId, touchState.WithLane(lane));
                return;
            }

            touchStateManager.Set(pointerId, touchState.WithLane(lane));
            judgeRequestController.Enqueue(FingerJudgeRequest.Down(lane));
        }

        private bool TryEnqueueHoldCross(int pointerId, int previousLane, int currentLane)
        {
            if (currentLane < 0 || previousLane == currentLane) return false;

            judgeRequestController.Enqueue(FingerJudgeRequest.HoldCross(pointerId, currentLane));
            return true;
        }

        private void FingerUp(int pointerId, Vector2 screenPosition)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return;

            if ((touchState.StartScreenPosition - screenPosition).sqrMagnitude > swipeThreshold * swipeThreshold)
            {
                if (TryEnqueueFingerSwipe(pointerId)) return;
            }

            var lane = touchState.Lane;
            judgeRequestController.Enqueue(FingerJudgeRequest.Up(lane));
            touchStateManager.Remove(pointerId);
        }

        private bool TryEnqueueFingerSwipe(int pointerId)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return false;

            judgeRequestController.Enqueue(FingerJudgeRequest.Swipe(touchState.Lane));
            touchStateManager.Remove(pointerId);
            return true;
        }

        private bool TryGetLane(Vector2 screenPosition, out int lane) => laneDetector.TryGetLane(screenPosition, out lane);

        private bool IsPointerOverGui(int pointerId) => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }
}
