#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Song
{
    public class LoadedChartInfo
    {
        public List<NoteData> NoteDataList { get; } = new();
        public HeaderData HeaderData { get; set; } = new();

        private int noteCount;

        public int NoteCount => noteCount;

        public void AddNoteData(float beatBegin, float beatEnd, int lane, NoteType noteType = NoteType.Single)
        {
            var tempo = HeaderData.Tempo / 60f;
            NoteDataList.Add(new()
            {
                BeatBegin = beatBegin,
                BeatEnd = beatEnd,
                SecBegin = beatBegin / tempo,
                SecEnd = beatEnd / tempo,
                Lane = lane,
                NoteType = noteType,
            });
            noteCount += noteType == NoteType.Long ? 2 : 1;
        }

        public void AddCurveNoteData(float beatBegin, float beatEnd, int lane, NoteType noteType, List<CurveSegment> curveSegments, float curveDuration, bool useMidPoint)
        {
            var tempo = HeaderData.Tempo / 60f;
            NoteDataList.Add(new()
            {
                BeatBegin = beatBegin,
                BeatEnd = beatEnd,
                SecBegin = beatBegin / tempo,
                SecEnd = beatEnd / tempo,
                Lane = lane,
                NoteType = noteType,
                CurveSegmentList = curveSegments,
                CurveDuration = curveDuration,
                UseMidPoint = useMidPoint,
            });
            noteCount += noteType == NoteType.Long ? 2 : 1;
        }
    }
}