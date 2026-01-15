using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace Song
{
    [Serializable]
    public class LoadedChartInfo
    {
        public ELoadResult LoadResult { get; set; }
        public List<NoteData> NoteDataList { get; private set; } = new();
        public HeaderData HeaderData { get; set; }

        public int NoteCount => NoteDataList.AsValueEnumerable().Select(x => x.NoteType.EnumEquals(ENoteType.Long) ? 2 : 1).Sum();

        public void AddNoteData(float beatBegin, float beatEnd, int lane, ENoteType noteType = ENoteType.Single) => NoteDataList.Add(new()
        {
            BeatBegin = beatBegin,
            BeatEnd = beatEnd,
            SecBegin = beatBegin / (HeaderData.Tempo / 60f),
            SecEnd = beatEnd / (HeaderData.Tempo / 60f),
            Lane = lane,
            NoteType = noteType,
        });
        public void AddCurveNoteData(float beatBegin, float beatEnd, int lane, ENoteType noteType, List<Vector2> curvePointList, float curveDuration) => NoteDataList.Add(new()
        {
            BeatBegin = beatBegin,
            BeatEnd = beatEnd,
            SecBegin = beatBegin / (HeaderData.Tempo / 60f),
            SecEnd = beatEnd / (HeaderData.Tempo / 60f),
            Lane = lane,
            NoteType = noteType,
            CurvePointList = curvePointList,
            CurveDuration = curveDuration,
            UseMidPoint = false,
        });
        public void AddCurveNoteData(float beatBegin, float beatEnd, int lane, ENoteType noteType, List<Vector2> curvePoints, float curveDuration, bool useMidPoint) => NoteDataList.Add(new()
        {
            BeatBegin = beatBegin,
            BeatEnd = beatEnd,
            SecBegin = beatBegin / (HeaderData.Tempo / 60f),
            SecEnd = beatEnd / (HeaderData.Tempo / 60f),
            Lane = lane,
            NoteType = noteType,
            CurvePointList = curvePoints,
            CurveDuration = curveDuration,
            UseMidPoint = useMidPoint,
        });
    }
}