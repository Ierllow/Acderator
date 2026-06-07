#nullable enable

using Zenject;

namespace Song
{
    public record NotePositionUpdateContext(float PositionBeginY, float PositionEndY, float CurrentSec);

    public class NotePositionUpdater
    {
        [Inject] private readonly NotesManager notesManager = default!;

        public void UpdatePositions()
        {
            var notes = notesManager.AliveNoteList;
            var currentBeat = notesManager.CurrentBeat;
            var currentSec = notesManager.CurrentSec;
            var currentNoteSpeed = notesManager.CurrentNoteSpeed;

            for (var i = notes.Count - 1; i >= 0; i--)
            {
                var note = notes[i];
                if (!note || !note.IsActive) continue;

                var noteData = notesManager.GetNoteData(note);
                var positionBeginY = note.IsTapping ? 0.0f : (noteData.BeatBegin - currentBeat) * currentNoteSpeed;
                var positionEndY = (noteData.BeatEnd - currentBeat) * currentNoteSpeed;

                note.UpdatePosition(new(positionBeginY, positionEndY, currentSec));
            }
        }
    }
}
