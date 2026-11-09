using Acderator.Shared;
using System.Linq;

namespace Song
{
    public static class ChartValidator
    {
        public static bool IsValid(ChartResponse response) => response.Tempo > 0
            && response.Notes != null
            && response.Notes.Count > 0
            && response.SpeedChanges != null
            && !response.Notes.Any(note => note.NoteType == (int)NoteType.Curve && note.Segments == null);
    }
}