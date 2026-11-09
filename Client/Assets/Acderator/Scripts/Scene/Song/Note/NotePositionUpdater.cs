#nullable enable

using Zenject;

namespace Song
{
    public readonly struct NotePositionUpdateContext
    {
        public float PositionBeginY { get; }
        public float PositionEndY { get; }
        public float CurrentSec { get; }

        public NotePositionUpdateContext(float positionBeginY, float positionEndY, float currentSec)
        {
            PositionBeginY = positionBeginY;
            PositionEndY = positionEndY;
            CurrentSec = currentSec;
        }
    }

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