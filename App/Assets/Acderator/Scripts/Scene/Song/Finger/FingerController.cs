using Intense;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Song
{
    public enum EFingerType { None, Up, Down }

    public class FingerController : MonoBehaviour, IController, IInitializable
    {
        private const int LaneNone = -1;

        [SerializeField] private Camera choose;
        [SerializeField] private Transform hitPlane;
        [SerializeField] private bool ignoreIsOverGui;
        [SerializeField] private bool ignoreStartedOverGui = true;
        [SerializeField] private float swipeThreshold = 50f;

        [Inject] private readonly NotesManager notesManager;
        [Inject] private readonly NoteFactory noteFactory;
        [Inject] private readonly NoteJudgeController noteJudgeController;
        [Inject] private readonly PointerInput pointerInput;
        [Inject] private readonly LaneDetector laneDetector;
        [Inject] private readonly TouchStateManager touchStateManager;

        private bool useTouch;

        private readonly Subject<FingerInfo> judgmentSubject = new();
        private readonly Subject<bool> useTouchSubject = new();

        public Observable<FingerInfo> JudgmentAsObservable => judgmentSubject;
        public Observable<bool> EveryUseTouchChanged => useTouchSubject.Where(x => !x);

        private bool IsAuto => notesManager.SongOption.IsAuto;

        public void Initialize()
        {
            if (IsAuto) return;
            laneDetector.Init(choose, hitPlane);
            pointerInput.SetCallbacks(FingerDown, FingerUpdate, FingerUp);
            TrySetUseTouch(true);
        }

        public void UpdateInput()
        {
            if (IsAuto || !useTouch) return;
            pointerInput.Update();
        }

        public void Judge(float currentSec)
        {
            var notes = notesManager.AliveNoteList;
            for (var i = notes.Count - 1; i >= 0; i--)
            {
                var note = notes[i];
                if (!note || !note.IsActive) continue;
                if (TryEmitMiss(note, currentSec)) continue;
                if (IsAuto) EmitAutoPerfect(note);
            }
        }

        public bool TrySetUseTouch(bool useTouch)
        {
            if (IsAuto || this.useTouch == useTouch) return false;

            this.useTouch = useTouch;
            if (!useTouch) touchStateManager.Clear();
            useTouchSubject.OnNext(useTouch);
            return true;
        }

        private void FingerDown(int pointerId, Vector2 screenPosition)
        {
            if ((ignoreStartedOverGui || ignoreIsOverGui) && IsPointerOverGui(pointerId)) return;

            var touchInPlane = laneDetector.GetWorldPosition(screenPosition);
            if (!laneDetector.TryGetLane(touchInPlane.x, touchInPlane.y, out var lane)) return;

            touchStateManager.Set(pointerId, new TouchStateManager.TouchState(screenPosition, lane));

            var fingerInfo = new FingerInfo { FingerType = EFingerType.Down, Lane = lane };
            if (notesManager.TryGetNote(EFingerType.Down, lane, out var note))
            {
                var diff = noteJudgeController.GetNoteDiffSec(EFingerType.Down, noteFactory.GetNoteData(note));
                if (diff < 0.5f) fingerInfo = ApplyJudgement(note, EFingerType.Down, lane, noteJudgeController.GetJudgmentType(diff));
            }
            judgmentSubject.OnNext(fingerInfo);
        }

        private void FingerUpdate(int pointerId, Vector2 screenPosition)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return;
            if (ignoreIsOverGui && IsPointerOverGui(pointerId)) return;

            var touchInPlane = laneDetector.GetWorldPosition(screenPosition);
            laneDetector.TryGetLane(touchInPlane.x, touchInPlane.y, out var lane);

            if (TryHandleHoldCross(pointerId, touchState.Lane, lane)) return;

            touchStateManager.Set(pointerId, touchState.WithLane(lane));
            judgmentSubject.OnNext(new FingerInfo { FingerType = EFingerType.Down, Lane = lane });
        }

        private bool TryHandleHoldCross(int pointerId, int previousLane, int currentLane)
        {
            if (currentLane < 0 || previousLane == currentLane) return false;
            if (!notesManager.TryGetNote(EFingerType.Up, currentLane, out var note)) return false;
            if (!note.IsTapping) return false;
            if (noteFactory.GetNoteData(note).NoteType is not (ENoteType.Long or ENoteType.Curve)) return false;

            var fingerInfo = ApplyJudgement(note, EFingerType.Up, currentLane, noteJudgeController.JudgeOrMiss(note, EFingerType.Up));
            touchStateManager.Remove(pointerId);

            judgmentSubject.OnNext(fingerInfo.WithTappingLanes(GetTappingLanes()));
            return true;
        }

        private void FingerUp(int pointerId, Vector2 screenPosition)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return;

            if ((touchState.StartScreenPosition - screenPosition).sqrMagnitude > swipeThreshold * swipeThreshold)
            {
                FingerSwipe(pointerId);
                if (!touchStateManager.TryGet(pointerId, out touchState)) return;
            }

            var lane = touchState.Lane;
            var fingerInfo = new FingerInfo { FingerType = EFingerType.Up, Lane = lane };

            if (notesManager.TryGetNote(EFingerType.Up, lane, out var note))
                fingerInfo = ApplyJudgement(note, EFingerType.Up, lane, noteJudgeController.JudgeOrMiss(note, EFingerType.Up));

            touchStateManager.Remove(pointerId);
            judgmentSubject.OnNext(fingerInfo);
        }

        private void FingerSwipe(int pointerId)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return;

            var lane = touchState.Lane;
            if (!notesManager.TryGetFlickNote(lane, out var note)) return;

            var fingerInfo = ApplyJudgement(note, EFingerType.Up, lane, noteJudgeController.JudgeOrMiss(note, EFingerType.Up));
            touchStateManager.Remove(pointerId);

            judgmentSubject.OnNext(fingerInfo);
        }

        private void EmitAutoPerfect(NoteBase note)
        {
            if (noteJudgeController.IsJustAutoTiming(note, EFingerType.Down))
            {
                EmitPerfect(note, EFingerType.Down);
                return;
            }
            if (noteFactory.GetNoteData(note).NoteType != ENoteType.Single && noteJudgeController.IsJustAutoTiming(note, EFingerType.Up))
                EmitPerfect(note, EFingerType.Up);
        }

        private bool TryEmitMiss(NoteBase note, float currentSec)
        {
            if (!noteJudgeController.IsMissed(note, currentSec, out var missEnd)) return false;
            EmitMiss(note, missEnd);
            return true;
        }

        private void EmitMiss(NoteBase note, bool missEnd)
        {
            if (notesManager.RemoveNote(note)) note.Final();

            judgmentSubject.OnNext(new FingerInfo
            {
                NoteBase = note,
                NoteData = noteFactory.GetNoteData(note),
                JudgmentType = IsAuto ? EJudgementType.Perfect : EJudgementType.Miss,
                Lane = LaneNone,
                MissInfo = (true, missEnd),
            });
        }

        private void EmitPerfect(NoteBase note, EFingerType fingerType)
        {
            note.OnJudgedNote(fingerType, EJudgementType.Perfect);
            judgmentSubject.OnNext(new FingerInfo
            {
                NoteBase = note,
                NoteData = noteFactory.GetNoteData(note),
                FingerType = fingerType,
                JudgmentType = EJudgementType.Perfect,
                Lane = LaneNone,
            });
        }

        private FingerInfo ApplyJudgement(NoteBase note, EFingerType fingerType, int lane, EJudgementType judgementType)
        {
            note.OnJudgedNote(fingerType, judgementType);
            return new FingerInfo
            {
                NoteBase = note,
                NoteData = noteFactory.GetNoteData(note),
                JudgmentType = judgementType,
                FingerType = fingerType,
                Lane = lane,
            };
        }

        private bool IsPointerOverGui(int pointerId) => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);

        private List<int> GetTappingLanes()
        {
            var aliveNotes = notesManager.AliveNoteList;
            var tappingLanes = new List<int>(aliveNotes.Count);
            for (var i = 0; i < aliveNotes.Count; i++)
            {
                var note = aliveNotes[i];
                if (!note.IsActive || !note.IsTapping) continue;
                if (!noteFactory.TryGetNoteData(note, out var data)) continue;
                tappingLanes.Add(data.Lane);
            }
            return tappingLanes;
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