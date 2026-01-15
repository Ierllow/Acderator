using Intense;
using R3;
using System;
using Zenject;
using ZLinq;

namespace Song
{
    public class AutoFingerController : IAutoFingerController
    {
        [Inject] private NotesManager notesManager;

        public Subject<FingerInfo> FingerInfoSubject { get; } = new();

        public void OnAutoFinger()
        {
            var aliveNotesList = notesManager.AliveNoteList;
            for (var i = aliveNotesList.Count - 1; i >= 0; i--)
            {
                var note = aliveNotesList.AsValueEnumerable().ElementAt(i);
                if (!note || !note.IsActive) continue;

                if (IsJustNoteTiming(note, EFingerType.Down))
                {
                    note.OnJudgedNote(EFingerType.Down, EJudgementType.Perfect);
                    NotifyFinger(new FingerInfo { NoteBase = note, FingerType = EFingerType.Down, JudgmentType = EJudgementType.Perfect });
                }
                else if (!note.NoteData.NoteType.EnumEquals(ENoteType.Single) && IsJustNoteTiming(note, EFingerType.Up))
                {
                    note.OnJudgedNote(EFingerType.Up, EJudgementType.Perfect);
                    NotifyFinger(new FingerInfo { NoteBase = note, FingerType = EFingerType.Up, JudgmentType = EJudgementType.Perfect });
                }
            }
        }

        private bool IsJustNoteTiming(NoteBase noteBase, EFingerType fingerType) => noteBase.NoteData.NoteType switch
        {
            ENoteType.Single => noteBase.NoteData.SecBegin <= notesManager.CurrentSec,
            ENoteType.Flick => fingerType.EnumEquals(EFingerType.Down)
                                ? noteBase.NoteData.SecBegin <= notesManager.CurrentSec && !noteBase.IsTapping
                                : noteBase.IsTapping,
            ENoteType.Long => fingerType.EnumEquals(EFingerType.Down)
                                ? noteBase.NoteData.SecBegin <= notesManager.CurrentSec && !noteBase.IsTapping
                                : noteBase.NoteData.SecEnd <= notesManager.CurrentSec && noteBase.IsTapping,
            ENoteType.Curve => fingerType.EnumEquals(EFingerType.Down)
                                ? noteBase.NoteData.SecBegin <= notesManager.CurrentSec && !noteBase.IsTapping
                                : noteBase.NoteData.SecBegin + noteBase.NoteData.CurveDuration <= notesManager.CurrentSec && noteBase.IsTapping,
            _ => false,
        };

        public void NotifyFinger(FingerInfo fingerInfo) => FingerInfoSubject.OnNext(fingerInfo);
    }
}