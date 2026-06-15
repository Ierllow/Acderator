#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Song
{
    public class LoadedChartInfo
    {
        public ELoadResult LoadResult { get; set; }
        public List<NoteData> NoteDataList { get; } = new();
        public HeaderData HeaderData { get; set; } = new();

        public int NoteCount => NoteDataList.Sum(x => x.NoteType == ENoteType.Long ? 2 : 1);

        public void AddNoteData(float beatBegin, float beatEnd, int lane, ENoteType noteType = ENoteType.Single) => NoteDataList.Add(new()
        {
            BeatBegin = beatBegin,
            BeatEnd = beatEnd,
            SecBegin = beatBegin / (HeaderData.Tempo / 60f),
            SecEnd = beatEnd / (HeaderData.Tempo / 60f),
            Lane = lane,
            NoteType = noteType,
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