using Intense;
using Intense.Master;
using System;
using Zenject;
using ZLinq;

namespace Song
{
    public class NoteUpdateOptimizer : IController
    {
        [Inject] private NotesManager notesManager;

        public void UpdatePositionNotes(Action<FingerInfo> notifyFinger)
        {
            var notes = notesManager.AliveNoteList;
            for (var i = notes.Count - 1; i >= 0; i--)
            {
                var note = notes[i];
                if (!note || !note.IsActive) continue;

                var positionBeginY = note.IsTapping ? 0.0f : (note.NoteData.BeatBegin - notesManager.CurrentBeat) * notesManager.CurrentNoteSpeed;
                var positionEndY = (note.NoteData.BeatEnd - notesManager.CurrentBeat) * notesManager.CurrentNoteSpeed;

                note.MoveNote(positionBeginY, positionEndY, notesManager.CurrentNoteSpeed);
                CheckMiss(note, notifyFinger);
            }
        }

        private void CheckMiss(NoteBase noteBase, Action<FingerInfo> notifyFinger)
        {
            if (!noteBase.NoteData.NoteType.EnumEquals(ENoteType.Long) &&
                !noteBase.NoteData.NoteType.EnumEquals(ENoteType.Curve)) return;

            var judgmentZone = MasterDataManager.Instance.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => x.Type <= EJudgementType.Bad.GetLength()).Zone;
            var noteDuration = noteBase.NoteData.NoteType.EnumEquals(ENoteType.Curve) ? noteBase.NoteData.CurveDuration : noteBase.NoteData.SecEnd - noteBase.NoteData.SecBegin;
            var isMissLongNoteBegin = !noteBase.IsTapping && noteBase.NoteData.SecBegin - notesManager.CurrentSec < -judgmentZone;
            if (isMissLongNoteBegin)
            {
                EmitMiss(noteBase, notifyFinger, missLongNote: true, missEnd: true);
                return;
            }

            var isMissNote = noteBase.NoteData.SecBegin - notesManager.CurrentSec < -judgmentZone;
            var isMissLongNoteEnd = noteBase.IsTapping && noteBase.NoteData.SecBegin + noteDuration - notesManager.CurrentSec < -judgmentZone;
            if (isMissNote || isMissLongNoteEnd)
            {
                EmitMiss(noteBase, notifyFinger, missLongNote: true, missEnd: false);
            }
        }

        private void EmitMiss(NoteBase noteBase, Action<FingerInfo> notifyFinger, bool missLongNote, bool missEnd)
        {
            if (notesManager.TryRemoveNote(noteBase)) noteBase.Final();

            notifyFinger.Invoke(new()
            {
                NoteBase = noteBase,
                JudgmentType = notesManager.SongOption.IsAuto ? EJudgementType.Perfect : EJudgementType.Miss,
                MissInfo = (missLongNote, missEnd)
            });
        }
    }
}