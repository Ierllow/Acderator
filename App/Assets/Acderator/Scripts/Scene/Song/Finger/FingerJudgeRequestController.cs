#nullable enable

using Intense;
using R3;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zenject;

namespace Song
{
    public sealed class FingerJudgeRequestController
    {
        private const int LaneNone = -1;

        [Inject] private readonly NotesManager notesManager = default!;
        [Inject] private readonly NoteJudgeController noteJudgeController = default!;
        [Inject] private readonly TouchStateManager touchStateManager = default!;

        private readonly List<FingerJudgeRequest> judgeRequestList = new();
        private readonly Subject<FingerInfo> judgmentSubject = new();

        public Observable<FingerInfo> JudgmentAsObservable => judgmentSubject;

        public void Enqueue(FingerJudgeRequest request)
        {
            judgeRequestList.RemoveAll(x => x.Frame != request.Frame);
            judgeRequestList.Add(request);
        }

        public void Clear() => judgeRequestList.Clear();

        public void Judge(float currentSec)
        {
            foreach (var judgeRequest in judgeRequestList.Where(x => x.IsCurrentFrame()))
            {
                EmitJudgeRequest(judgeRequest);
            }
            judgeRequestList.Clear();
            var notes = notesManager.AliveNoteList;
            for (var i = notes.Count - 1; i >= 0; i--)
            {
                var note = notes[i];
                if (!note || !note.IsActive) continue;
                if (noteJudgeController.IsMissed(note, currentSec)) { EmitMiss(note, currentSec); continue; }
                if (notesManager.SongOption.IsAuto) EmitPerfect(note);
            }
        }

        private void EmitJudgeRequest(FingerJudgeRequest request)
        {
            switch (request.RequestType)
            {
                case FingerJudgeRequestType.Swipe:
                    EmitSwipeJudgeRequest(request);
                    break;
                case FingerJudgeRequestType.HoldCross:
                    EmitHoldCrossJudgeRequest(request);
                    break;
                default:
                    EmitNormalJudgeRequest(request);
                    break;
            }
        }

        private void EmitNormalJudgeRequest(FingerJudgeRequest request)
        {
            var fingerInfo = new FingerInfo() { FingerType = request.FingerType, Lane = request.Lane };
            if (TryGetRequestedNote(request, out var note))
            {
                TryApplyJudgement(note, request.FingerType, request.Lane, request.AllowMiss, out fingerInfo);
                judgmentSubject.OnNext(fingerInfo);
            }
        }

        private void EmitSwipeJudgeRequest(FingerJudgeRequest request)
        {
            if (notesManager.TryGetFlickNote(request.Lane, out var flickNote) && TryApplyJudgement(flickNote, EFingerType.Up, request.Lane, false, out var fingerInfo))
            {
                judgmentSubject.OnNext(fingerInfo);
                return;
            }
            EmitNormalJudgeRequest(FingerJudgeRequest.Up(request.Lane));
        }

        private void EmitHoldCrossJudgeRequest(FingerJudgeRequest request)
        {
            if (!notesManager.TryGetNote(EFingerType.Up, request.Lane, out var note)) return;
            if (!TryApplyJudgement(note, EFingerType.Up, request.Lane, true, out var fingerInfo)) return;

            touchStateManager.Remove(request.PointerId);
            judgmentSubject.OnNext(fingerInfo.WithTappingLanes(GetTappingLanes()));
        }

        private bool TryGetRequestedNote(FingerJudgeRequest request, [NotNullWhen(true)] out NoteBase? note) => notesManager.TryGetNote(request.FingerType, request.Lane, out note);

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

        private void EmitMiss(NoteBase note, float currentSec) => judgmentSubject.OnNext(new()
        {
            NoteBase = note,
            NoteData = notesManager.GetNoteData(note),
            JudgmentType = notesManager.SongOption.IsAuto ? EJudgementType.Perfect : EJudgementType.Miss,
            Lane = LaneNone,
            MissInfo = (true, true),
        });

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
            fingerInfo = new FingerInfo() { FingerType = fingerType, Lane = lane };
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

        private List<int> GetTappingLanes() => notesManager.AliveNoteList.Where(note => note && note.IsActive && note.IsTapping).Select(x => notesManager.GetNoteData(x).Lane).ToList();
    }
}