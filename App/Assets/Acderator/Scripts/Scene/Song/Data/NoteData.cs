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
        public ENoteType NoteType { get; init; }
        public List<Vector2> CurvePointList { get; init; }
        public float CurveDuration { get; init; }
        public bool UseMidPoint { get; init; }
    }
}