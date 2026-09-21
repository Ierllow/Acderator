#nullable enable

using Intense;
using System.Collections.Generic;

namespace Song
{
    public readonly struct FingerInfo
    {
        public NoteBase? NoteBase { get; init; }
        public NoteData? NoteData { get; init; }
        public JudgementType JudgmentType { get; init; }
        public FingerType FingerType { get; init; }
        public int Lane { get; init; }
        public (bool afterMiss, bool missLongNote)? MissInfo { get; init; }
        public IReadOnlyList<int>? TappingLanes { get; init; }

        public bool IsMissed => MissInfo?.afterMiss ?? false;

        public bool IsFlick => NoteData?.NoteType == NoteType.Flick;

        public bool IsMiss => JudgmentType == JudgementType.Miss;

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