#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Song
{
    [System.Serializable]
    public class NoteData
    {
        public float BeatBegin { get; init; }
        public float BeatEnd { get; init; }
        public float SecBegin { get; init; }
        public float SecEnd { get; init; }
        public int Lane { get; init; }
        public NoteType NoteType { get; init; }
        public List<CurveSegment> CurveSegmentList { get; init; } = new();
        public float CurveDuration { get; init; }
        public bool UseMidPoint { get; init; }
    }

    [System.Serializable]
    public class CurveSegment
    {
        public List<Vector2> Points { get; init; } = new();
        public float StartT { get; init; }
        public float EndT { get; init; }
    }
}