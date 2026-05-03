using Intense;
using Intense.Master;
using System;
using Zenject;

namespace Song
{
    public class NoteUpdateOptimizer : IController
    {
        [Inject] private MasterDataManager masterDataManager;
        [Inject] private NotesManager notesManager;

        private float? badJudgmentZone;

        private float BadJudgmentZone => badJudgmentZone ??= masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => x.Type <= (int)EJudgementType.Bad).Zone;

        public void UpdatePositionNotes(Action<FingerInfo> notifyFinger)
        {
            var notes = notesManager.AliveNoteList;
            var currentBeat = notesManager.CurrentBeat;
            var currentSec = notesManager.CurrentSec;
            var currentNoteSpeed = notesManager.CurrentNoteSpeed;

            for (var i = notes.Count - 1; i >= 0; i--)
            {
                var note = notes[i];
                if (!note || !note.IsActive) continue;

                var noteData = note.NoteData;
                var positionBeginY = note.IsTapping ? 0.0f : (noteData.BeatBegin - currentBeat) * currentNoteSpeed;
                var positionEndY = (noteData.BeatEnd - currentBeat) * currentNoteSpeed;

                note.MoveNote(positionBeginY, positionEndY, currentNoteSpeed);
                CheckMiss(note, currentSec, notifyFinger);
            }
        }

        private void CheckMiss(NoteBase noteBase, float currentSec, Action<FingerInfo> notifyFinger)
        {
            var noteType = noteBase.NoteData.NoteType;
            var isLong = noteType == ENoteType.Long;
            var isCurve = noteType == ENoteType.Curve;
            if (!isLong && !isCurve) return;

            var judgmentZone = BadJudgmentZone;
            var noteData = noteBase.NoteData;
            var noteDuration = isCurve
                ? noteData.CurveDuration
                : noteData.SecEnd - noteData.SecBegin;

            var isMissLongNoteBegin = !noteBase.IsTapping && noteData.SecBegin - currentSec < -judgmentZone;
            if (isMissLongNoteBegin)
            {
                EmitMiss(noteBase, notifyFinger, missLongNote: true, missEnd: true);
                return;
            }

            var isMissNote = noteData.SecBegin - currentSec < -judgmentZone;
            var isMissLongNoteEnd = noteBase.IsTapping && noteData.SecBegin + noteDuration - currentSec < -judgmentZone;
            if (isMissNote || isMissLongNoteEnd)
            {
                EmitMiss(noteBase, notifyFinger, missLongNote: true, missEnd: false);
            }
        }

        private void EmitMiss(NoteBase noteBase, Action<FingerInfo> notifyFinger, bool missLongNote, bool missEnd)
        {
            if (notesManager.RemoveNote(noteBase)) noteBase.Final();

            notifyFinger.Invoke(new FingerInfo
            {
                NoteBase = noteBase,
                JudgmentType = notesManager.SongOption.IsAuto ? EJudgementType.Perfect : EJudgementType.Miss,
                MissInfo = (missLongNote, missEnd)
            });
        }
    }
}
