#nullable enable

using System.Collections.Generic;

namespace Song
{
    public class HeaderData
    {
        public List<NoteSpeedChange> NoteSpeedChangeList { get; init; } = new();
        public int Tempo { get; init; }
    }
}
