using Zenject;

namespace Song
{
    public class NotePositionUpdater : IController
    {
        [Inject] private readonly NotesManager notesManager;
        [Inject] private readonly NoteFactory noteFactory;

        public void UpdatePositions()
        {
            var notes = notesManager.AliveNoteList;
            var currentBeat = notesManager.CurrentBeat;
            var currentNoteSpeed = notesManager.CurrentNoteSpeed;

            for (var i = notes.Count - 1; i >= 0; i--)
            {
                var note = notes[i];
                if (!note || !note.IsActive) continue;
                if (!noteFactory.TryGetNoteData(note, out var noteData)) continue;

                var positionBeginY = note.IsTapping ? 0.0f : (noteData.BeatBegin - currentBeat) * currentNoteSpeed;
                var positionEndY = (noteData.BeatEnd - currentBeat) * currentNoteSpeed;

                note.MoveNote(positionBeginY, positionEndY, currentNoteSpeed);
            }
        }
    }
}