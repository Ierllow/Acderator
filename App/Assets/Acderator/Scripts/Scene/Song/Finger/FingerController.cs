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
            laneDetector.Init(choose, hitPlane, noteFactory.LaneParents);
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
                if (IsAuto) EmitPerfect(note);
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

            if (!TryGetLane(screenPosition, out var lane)) return;

            touchStateManager.Set(pointerId, new TouchStateManager.TouchState(screenPosition, lane));

            EmitLaneInput(EFingerType.Down, lane);
        }

        private void FingerUpdate(int pointerId, Vector2 screenPosition)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return;
            if (ignoreIsOverGui && IsPointerOverGui(pointerId)) return;

            TryGetLane(screenPosition, out var lane);

            if (TryHandleHoldCross(pointerId, touchState.Lane, lane)) return;

            touchStateManager.Set(pointerId, touchState.WithLane(lane));
            EmitLaneInput(EFingerType.Down, lane);
        }

        private bool TryHandleHoldCross(int pointerId, int previousLane, int currentLane)
        {
            if (currentLane < 0 || previousLane == currentLane) return false;
            if (!notesManager.TryGetNote(EFingerType.Up, currentLane, out var note)) return false;

            if (!TryApplyJudgement(note, EFingerType.Up, currentLane, true, out var fingerInfo)) return false;
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
            var fingerInfo = CreateFingerInfo(EFingerType.Up, lane);
            if (notesManager.TryGetNote(EFingerType.Up, lane, out var note))
            {
                TryApplyJudgement(note, EFingerType.Up, lane, true, out fingerInfo);
            }

            touchStateManager.Remove(pointerId);
            judgmentSubject.OnNext(fingerInfo);
        }

        private void FingerSwipe(int pointerId)
        {
            if (!touchStateManager.TryGet(pointerId, out var touchState)) return;

            var lane = touchState.Lane;
            if (!notesManager.TryGetFlickNote(lane, out var note)) return;
            if (!TryApplyJudgement(note, EFingerType.Up, lane, false, out var fingerInfo)) return;

            touchStateManager.Remove(pointerId);

            judgmentSubject.OnNext(fingerInfo);
        }

        private bool TryGetLane(Vector2 screenPosition, out int lane) => laneDetector.TryGetLane(screenPosition, out lane);

        private void EmitLaneInput(EFingerType fingerType, int lane)
        {
            var fingerInfo = CreateFingerInfo(fingerType, lane);
            if (notesManager.TryGetNote(fingerType, lane, out var note))
            {
                TryApplyJudgement(note, fingerType, lane, false, out fingerInfo);
            }
            judgmentSubject.OnNext(fingerInfo);
        }

        private void EmitPerfect(NoteBase note)
        {
            if (noteJudgeController.IsJustAutoTiming(note, EFingerType.Down))
            {
                EmitPerfect(note, EFingerType.Down);
                return;
            }
            if (notesManager.GetNoteData(note).NoteType != ENoteType.Single && noteJudgeController.IsJustAutoTiming(note, EFingerType.Up))
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
            var noteData = notesManager.GetNoteData(note);
            if (notesManager.RemoveNote(note)) note.Final();

            judgmentSubject.OnNext(new FingerInfo
            {
                NoteBase = note,
                NoteData = noteData,
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
                NoteData = notesManager.GetNoteData(note),
                FingerType = fingerType,
                JudgmentType = EJudgementType.Perfect,
                Lane = LaneNone,
            });
        }

        private bool TryApplyJudgement(NoteBase note, EFingerType fingerType, int lane, bool allowMiss, out FingerInfo fingerInfo)
        {
            fingerInfo = CreateFingerInfo(fingerType, lane);
            if (!noteJudgeController.TryJudge(note, fingerType, allowMiss, out var judgementType)) return false;

            note.OnJudgedNote(fingerType, judgementType);
            fingerInfo = new FingerInfo
            {
                NoteBase = note,
                NoteData = notesManager.GetNoteData(note),
                JudgmentType = judgementType,
                FingerType = fingerType,
                Lane = lane,
            };
            return true;
        }

        private FingerInfo CreateFingerInfo(EFingerType fingerType, int lane) => new() { FingerType = fingerType, Lane = lane };

        private bool IsPointerOverGui(int pointerId) => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);

        private List<int> GetTappingLanes()
        {
            var aliveNotes = notesManager.AliveNoteList;
            var tappingLanes = new List<int>(aliveNotes.Count);
            for (var i = 0; i < aliveNotes.Count; i++)
            {
                var note = aliveNotes[i];
                if (!note.IsActive || !note.IsTapping) continue;
                tappingLanes.Add(notesManager.GetNoteData(note).Lane);
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