using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Master;
using Lean.Common;
using Lean.Touch;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Song
{
    public enum EFingerType { None, Up, Down }

    public class FingerController : MonoBehaviour, IFingeController
    {
        [SerializeField] private LeanTouch leanTouch;
        [SerializeField] private LeanSelectable requiredSelectable;
        [SerializeField] private Camera choose;
        [SerializeField] private Transform hitPlane;
        [SerializeField] private bool ignoreIsOverGui;
        [SerializeField] private bool ignoreStartedOverGui = true;

        [Inject] private NotesManager notesManager;
        [Inject] private MasterDataManager masterDataManager;

        private readonly Plane touchPlane = new();
        private float dist;
        private readonly Dictionary<LeanFinger, int> fingerLaneDict = new();

        public Subject<(FingerInfo, int)> FingerInfoSubject { get; } = new();

        public IUniTaskAsyncEnumerable<bool> EveryUseTouchChanged => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.leanTouch.UseTouch).Where(x => !x);

        public void Init()
        {
            touchPlane.SetNormalAndPosition(hitPlane.transform.forward, hitPlane.transform.position);
            dist = Vector3.Distance(choose.transform.position, hitPlane.position);

            LeanTouch.OnFingerDown += FingerDown;
            LeanTouch.OnFingerUpdate += FingerUpdate;
            LeanTouch.OnFingerUp += FingerUp;
            LeanTouch.OnFingerSwipe += FingerSwipe;

            TrySetUseTouch(true);
        }

        private void FingerDown(LeanFinger finger)
        {
            if (ignoreStartedOverGui && finger.StartedOverGui) return;
            if (ignoreIsOverGui && finger.IsOverGui) return;
            if (requiredSelectable != null && !requiredSelectable) return;

            var touchInPlane = finger.GetWorldPosition(dist, choose);
            var fingerInfo = new FingerInfo { FingerType = EFingerType.Down };
            if (TryGetTouchLane(touchInPlane.x, touchInPlane.y, out var lane))
            {
                fingerLaneDict[finger] = lane;

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
            if (noteData.NoteType.EnumEquals(ENoteType.Curve))
            {
                var curveProgress = Mathf.Clamp01((notesManager.CurrentSec - noteData.SecBegin) / noteData.CurveDuration);
                return notesManager.GetCurveNoteDiffSec(noteData, curveProgress);
            }
            return notesManager.GetDiffSec(fingerType, noteData);
        }

        private void FingerUpdate(LeanFinger finger)
        {
            if (!fingerLaneDict.TryGetValue(finger, out var previousLane)) return;

            var fingerInfo = new FingerInfo { FingerType = EFingerType.Down };
            var touchInPlane = finger.GetWorldPosition(dist, choose);

            if (TryGetTouchLane(touchInPlane.x, touchInPlane.y, out var lane)
                && previousLane != lane
                && notesManager.TryGetNote(EFingerType.Up, lane, out var note)
                && note.IsTapping
                && (note.NoteData.NoteType.EnumEquals(ENoteType.Long) || note.NoteData.NoteType.EnumEquals(ENoteType.Curve)))
            {
                var judgementType = GetNoteDiffSec(EFingerType.Up, note.NoteData).GetJudgmentType(masterDataManager);
                judgementType = !judgementType.EnumEquals(EJudgementType.None) ? judgementType : EJudgementType.Miss;

                note.OnJudgedNote(EFingerType.Up, judgementType);

                fingerInfo = new FingerInfo
                {
                    NoteBase = note,
                    JudgmentType = judgementType,
                    FingerType = EFingerType.Up,
                    TappingNoteList = notesManager.AliveNoteList.Where(x => x.IsActive && x.IsTapping).ToList()
                };

                fingerLaneDict.Remove(finger);
            }
            else
            {
                fingerLaneDict[finger] = lane;
            }

            NotifyFinger(fingerInfo, lane);
        }

        private void FingerUp(LeanFinger finger)
        {
            if (!fingerLaneDict.TryGetValue(finger, out var lane)) return;

            var fingerInfo = new FingerInfo { FingerType = EFingerType.Up };

            if (notesManager.TryGetNote(EFingerType.Up, lane, out var note))
            {
                var judgementType = GetNoteDiffSec(EFingerType.Up, note.NoteData).GetJudgmentType(masterDataManager);
                judgementType = !judgementType.EnumEquals(EJudgementType.None) ? judgementType : EJudgementType.Miss;

                note.OnJudgedNote(EFingerType.Up, judgementType);
                fingerInfo = new FingerInfo
                {
                    NoteBase = note,
                    JudgmentType = judgementType,
                    FingerType = EFingerType.Up
                };
            }

            fingerLaneDict.Remove(finger);
            NotifyFinger(fingerInfo, lane);
        }

        private void FingerSwipe(LeanFinger finger)
        {
            if (!fingerLaneDict.TryGetValue(finger, out var lane)) return;
            if (!notesManager.TryGetFlickNote(lane, out var note)) return;

            var diff = GetNoteDiffSec(EFingerType.Up, note.NoteData);
            var judgementType = diff.GetJudgmentType(masterDataManager);
            judgementType = !judgementType.EnumEquals(EJudgementType.None) ? judgementType : EJudgementType.Miss;

            note.OnJudgedNote(EFingerType.Up, judgementType);

            NotifyFinger(new()
            {
                NoteBase = note,
                JudgmentType = judgementType,
                FingerType = EFingerType.Up
            }, lane);

            fingerLaneDict.Remove(finger);
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

        public bool TrySetUseTouch(bool useTouch) => leanTouch.UseTouch != useTouch && (leanTouch.UseTouch = useTouch) || Application.isEditor && (leanTouch.UseMouse = useTouch) && (leanTouch.UseTouch = useTouch) == useTouch;

        private void OnDestroy()
        {
            LeanTouch.OnFingerDown -= FingerDown;
            LeanTouch.OnFingerUpdate -= FingerUpdate;
            LeanTouch.OnFingerUp -= FingerUp;
            LeanTouch.OnFingerSwipe -= FingerSwipe;
        }
    }
}