using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Master;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Song
{
    public enum EFingerType { None, Up, Down }

    public class FingerController : MonoBehaviour, IFingeController
    {
        private readonly struct TouchState
        {
            public TouchState(Vector2 startScreenPosition, int lane)
            {
                StartScreenPosition = startScreenPosition;
                Lane = lane;
            }

            public Vector2 StartScreenPosition { get; }
            public int Lane { get; }

            public TouchState WithLane(int lane) => new(StartScreenPosition, lane);
        }

        [SerializeField] private Camera choose;
        [SerializeField] private Transform hitPlane;
        [SerializeField] private bool ignoreIsOverGui;
        [SerializeField] private bool ignoreStartedOverGui = true;
        [SerializeField] private float swipeThreshold = 50f;

        [Inject] private NotesManager notesManager;
        [Inject] private MasterDataManager masterDataManager;
        [Inject] private PointerInput pointerInput;

        private float dist;
        private bool useTouch;
        private readonly Dictionary<int, TouchState> touchStateDict = new();

        public Subject<(FingerInfo, int)> FingerInfoSubject { get; } = new();

        public IUniTaskAsyncEnumerable<bool> EveryUseTouchChanged => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.useTouch).Where(x => !x);

        private void Start() => UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ => UpdatePointerInput()).RegisterTo(destroyCancellationToken);

        public void Init()
        {
            dist = Vector3.Distance(choose.transform.position, hitPlane.position);
            pointerInput.SetCallbacks(FingerDown, FingerUpdate, FingerUp);
            TrySetUseTouch(true);
        }

        private void UpdatePointerInput()
        {
            if (!useTouch) return;

            pointerInput.Update();
        }

        private void FingerDown(int pointerId, Vector2 screenPosition)
        {
            var isOverGui = IsPointerOverGui(pointerId);
            if (ignoreStartedOverGui && isOverGui) return;
            if (ignoreIsOverGui && isOverGui) return;

            var touchInPlane = GetWorldPosition(screenPosition);
            var fingerInfo = new FingerInfo { FingerType = EFingerType.Down };
            if (TryGetTouchLane(touchInPlane.x, touchInPlane.y, out var lane))
            {
                touchStateDict[pointerId] = new TouchState(screenPosition, lane);

                if (notesManager.TryGetNote(EFingerType.Down, lane, out var note))
                {
                    var diff = GetNoteDiffSec(EFingerType.Down, note.NoteData);
                    if (diff < 0.5f)
                    {
                        var judgementType = diff.GetJudgmentType(masterDataManager);
                        note.OnJudgedNote(EFingerType.Down, judgementType);
                        fingerInfo = new FingerInfo
                        {
                            NoteBase = note,
                            JudgmentType = judgementType,
                            FingerType = EFingerType.Down
                        };
                    }
                }
                NotifyFinger(fingerInfo, lane);
            }
        }

        private float GetNoteDiffSec(EFingerType fingerType, NoteData noteData)
        {
            if (noteData.NoteType == ENoteType.Curve)
            {
                var curveProgress = Mathf.Clamp01((notesManager.CurrentSec - noteData.SecBegin) / noteData.CurveDuration);
                return notesManager.GetCurveNoteDiffSec(noteData, curveProgress);
            }
            return notesManager.GetDiffSec(fingerType, noteData);
        }

        private void FingerUpdate(int pointerId, Vector2 screenPosition)
        {
            if (!touchStateDict.TryGetValue(pointerId, out var touchState)) return;
            if (ignoreIsOverGui && IsPointerOverGui(pointerId)) return;

            var fingerInfo = new FingerInfo { FingerType = EFingerType.Down };
            var touchInPlane = GetWorldPosition(screenPosition);

            if (TryGetTouchLane(touchInPlane.x, touchInPlane.y, out var lane)
                && touchState.Lane != lane
                && notesManager.TryGetNote(EFingerType.Up, lane, out var note)
                && note.IsTapping
                && (note.NoteData.NoteType == ENoteType.Long || note.NoteData.NoteType == ENoteType.Curve))
            {
                var judgementType = GetNoteDiffSec(EFingerType.Up, note.NoteData).GetJudgmentType(masterDataManager);
                judgementType = judgementType != EJudgementType.None ? judgementType : EJudgementType.Miss;

                note.OnJudgedNote(EFingerType.Up, judgementType);

                fingerInfo = new FingerInfo
                {
                    NoteBase = note,
                    JudgmentType = judgementType,
                    FingerType = EFingerType.Up,
                    TappingNoteList = CreateTappingNoteList()
                };

                touchStateDict.Remove(pointerId);
            }
            else
            {
                touchStateDict[pointerId] = touchState.WithLane(lane);
            }

            NotifyFinger(fingerInfo, lane);
        }

        private void FingerUp(int pointerId, Vector2 screenPosition)
        {
            if (!touchStateDict.TryGetValue(pointerId, out var touchState)) return;

            if ((touchState.StartScreenPosition - screenPosition).sqrMagnitude > swipeThreshold * swipeThreshold)
            {
                FingerSwipe(pointerId);
                if (!touchStateDict.TryGetValue(pointerId, out touchState)) return;
            }

            var lane = touchState.Lane;
            var fingerInfo = new FingerInfo { FingerType = EFingerType.Up };

            if (notesManager.TryGetNote(EFingerType.Up, lane, out var note))
            {
                var judgementType = GetNoteDiffSec(EFingerType.Up, note.NoteData).GetJudgmentType(masterDataManager);
                judgementType = judgementType != EJudgementType.None ? judgementType : EJudgementType.Miss;

                note.OnJudgedNote(EFingerType.Up, judgementType);
                fingerInfo = new FingerInfo
                {
                    NoteBase = note,
                    JudgmentType = judgementType,
                    FingerType = EFingerType.Up
                };
            }

            touchStateDict.Remove(pointerId);
            NotifyFinger(fingerInfo, lane);
        }

        private void FingerSwipe(int pointerId)
        {
            if (!touchStateDict.TryGetValue(pointerId, out var touchState)) return;

            var lane = touchState.Lane;
            if (!notesManager.TryGetFlickNote(lane, out var note)) return;

            var diff = GetNoteDiffSec(EFingerType.Up, note.NoteData);
            var judgementType = diff.GetJudgmentType(masterDataManager);
            judgementType = judgementType != EJudgementType.None ? judgementType : EJudgementType.Miss;

            note.OnJudgedNote(EFingerType.Up, judgementType);

            NotifyFinger(new()
            {
                NoteBase = note,
                JudgmentType = judgementType,
                FingerType = EFingerType.Up
            }, lane);

            touchStateDict.Remove(pointerId);
        }

        private Vector3 GetWorldPosition(Vector2 screenPosition) => choose.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, dist));

        private static bool IsPointerOverGui(int pointerId) => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);

        private List<NoteBase> CreateTappingNoteList()
        {
            var aliveNotes = notesManager.AliveNoteList;
            var tappingNotes = new List<NoteBase>(aliveNotes.Count);
            for (var i = 0; i < aliveNotes.Count; i++)
            {
                var note = aliveNotes[i];
                if (note.IsActive && note.IsTapping) tappingNotes.Add(note);
            }
            return tappingNotes;
        }

        private bool TryGetTouchLane(float positionX, float positionY, out int lane)
        {
            lane = positionX >= -6.8f && positionX <= -2.8f && positionY <= -1.2f && positionY >= -4.5f
                ? 0 : positionX >= -2.8f && positionX <= 0.0f && positionY <= -1.2f && positionY >= -4.5f
                ? 1 : positionX >= 0.0f && positionX <= 2.8f && positionY <= -1.2f && positionY >= -4.5f
                ? 2 : positionX >= 2.8f && positionX <= 6.8f && positionY <= -1.2f && positionY >= -4.5f
                ? 3 : -1;
            return lane >= 0;
        }

        public void NotifyFinger(FingerInfo fingerInfo, int lane) => FingerInfoSubject.OnNext((fingerInfo, lane));

        public bool TrySetUseTouch(bool useTouch)
        {
            if (this.useTouch == useTouch) return false;

            this.useTouch = useTouch;
            if (!useTouch) touchStateDict.Clear();
            return true;
        }
    }

    internal sealed class PointerInput
    {
        private const int MousePointerId = -1;

        private Action<int, Vector2> fingerDown;
        private Action<int, Vector2> fingerUpdate;
        private Action<int, Vector2> fingerUp;

        public void SetCallbacks(Action<int, Vector2> fingerDown, Action<int, Vector2> fingerUpdate, Action<int, Vector2> fingerUp)
        {
            this.fingerDown = fingerDown;
            this.fingerUpdate = fingerUpdate;
            this.fingerUp = fingerUp;
        }

        public void Update()
        {
            var touchCount = Input.touchCount;
            if (Input.touchSupported && touchCount > 0)
            {
                UpdateTouches(touchCount);
                return;
            }

            if (Application.isEditor) UpdateMouse();
        }

        private void UpdateTouches(int touchCount)
        {
            for (var i = 0; i < touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        fingerDown(touch.fingerId, touch.position);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        fingerUpdate(touch.fingerId, touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        fingerUp(touch.fingerId, touch.position);
                        break;
                }
            }
        }

        private void UpdateMouse()
        {
            var screenPosition = (Vector2)Input.mousePosition;
            if (Input.GetMouseButtonDown(0)) fingerDown(MousePointerId, screenPosition);
            else if (Input.GetMouseButton(0)) fingerUpdate(MousePointerId, screenPosition);
            else if (Input.GetMouseButtonUp(0)) fingerUp(MousePointerId, screenPosition);
        }
    }
}