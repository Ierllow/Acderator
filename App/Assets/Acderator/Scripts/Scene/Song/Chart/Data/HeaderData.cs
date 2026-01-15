using System.Collections.Generic;

namespace Song
{
    [System.Serializable]
    public class HeaderData
    {
        public List<NoteSpeedChange> NoteSpeedChangeList { get; init; }
        public int Tempo { get; init; }
    }
}