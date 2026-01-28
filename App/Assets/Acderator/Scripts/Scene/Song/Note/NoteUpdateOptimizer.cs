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

        public void UpdatePositionNotes(Action<FingerInfo> notifyFinger) => UpdateNotes(notifyFinger);

        private void UpdateNotes(Action<FingerInfo> notifyFinger)
        {
            var count = notesManager.AliveNoteList.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var note = notesManager.AliveNoteList.AsValueEnumerable().ElementAt(i);
                if (!note || !note.IsActive) continue;

                var positionBeginY = note.IsTapping ? 0.0f : (note.NoteData.BeatBegin - notesManager.CurrentBeat) * notesManager.CurrentNoteSpeed;
                var positionEndY = (note.NoteData.BeatEnd - notesManager.CurrentBeat) * notesManager.CurrentNoteSpeed;
                note.MoveNote(positionBeginY, positionEndY, notesManager.CurrentNoteSpeed);
                MissNote(note, notifyFinger);
            }
        }

        private void MissNote(NoteBase noteBase, Action<FingerInfo> notifyFinger)
        {
            if (!notesManager.SongOption.IsAuto) return;
            if (!noteBase.NoteData.NoteType.EnumEquals(ENoteType.Long) && !noteBase.NoteData.NoteType.EnumEquals(ENoteType.Curve)) return;

            var judgmentZone = MasterDataManager.Instance.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => x.Type <= EJudgementType.Bad.GetLength()).Zone;
            var perfectOrMiss = notesManager.SongOption.IsAuto ? EJudgementType.Perfect : EJudgementType.Miss;
            var noteDuration = noteBase.NoteData.NoteType.EnumEquals(ENoteType.Curve) ? noteBase.NoteData.CurveDuration : noteBase.NoteData.SecEnd - noteBase.NoteData.SecBegin;

            var isMissLongNoteBegin = !noteBase.IsTapping && noteBase.NoteData.SecBegin - notesManager.CurrentSec < -judgmentZone;
            if (isMissLongNoteBegin)
            {
                if (notesManager.TryRemoveNote(noteBase)) noteBase.Final();
                notifyFinger.Invoke(new() { NoteBase = noteBase, JudgmentType = perfectOrMiss, MissInfo = (true, true) });
                return;
            }

            var isMissNote = noteBase.NoteData.SecBegin - notesManager.CurrentSec < -judgmentZone;
            var isMissLongNoteEnd = noteBase.IsTapping && noteBase.NoteData.SecBegin + noteDuration - notesManager.CurrentSec < -judgmentZone;
            if (isMissNote || isMissLongNoteEnd)
            {
                if (notesManager.TryRemoveNote(noteBase)) noteBase.Final();
                notifyFinger.Invoke(new() { NoteBase = noteBase, JudgmentType = perfectOrMiss, MissInfo = (true, false) });
            }
        }
    }
}