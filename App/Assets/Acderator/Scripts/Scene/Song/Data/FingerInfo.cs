using Intense;
using System;
using System.Collections.Generic;

namespace Song
{
    public readonly struct FingerInfo
    {
        public NoteBase NoteBase { get; init; }
        public EJudgementType JudgmentType { get; init; }
        public EFingerType FingerType { get; init; }
        public (bool afterMiss, bool missLongNote)? MissInfo { get; init; }
        public List<NoteBase> TappingNoteList { get; init; }

        public bool IsMissed => MissInfo?.afterMiss ?? false;

        public bool IsFlick => NoteBase?.NoteData?.NoteType.EnumEquals(ENoteType.Flick) ?? false;

        public bool IsMiss => JudgmentType.EnumEquals(EJudgementType.Miss);
    }
}