using Intense;
using System.Collections.Generic;

namespace Song
{
    public readonly struct FingerInfo
    {
        public NoteBase NoteBase { get; init; }
        public NoteData NoteData { get; init; }
        public EJudgementType JudgmentType { get; init; }
        public EFingerType FingerType { get; init; }
        public int Lane { get; init; }
        public (bool afterMiss, bool missLongNote)? MissInfo { get; init; }
        public IReadOnlyList<int> TappingLanes { get; init; }

        public bool IsMissed => MissInfo?.afterMiss ?? false;

        public bool IsFlick => NoteData?.NoteType == ENoteType.Flick;

        public bool IsMiss => JudgmentType == EJudgementType.Miss;

        public FingerInfo WithTappingLanes(IReadOnlyList<int> tappingLanes) => new()
        {
            NoteBase = NoteBase,
            NoteData = NoteData,
            JudgmentType = JudgmentType,
            FingerType = FingerType,
            Lane = Lane,
            MissInfo = MissInfo,
            TappingLanes = tappingLanes,
        };
    }
}