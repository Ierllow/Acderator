using Acderator.Shared;
using System.Linq;
using UnityEngine;

namespace Song
{
    public static class ChartResponseExtensions
    {
        public static LoadedChartInfo ToLoadedChartInfo(this ChartResponse response)
        {
            var loadedChartInfo = new LoadedChartInfo
            {
                HeaderData = new HeaderData
                {
                    Tempo = response.Tempo,
                    NoteSpeedChangeList = response.SpeedChanges.Select(s => new NoteSpeedChange { Beat = s.Beat, Speed = s.Speed }).ToList(),
                },
            };

            foreach (var note in response.Notes)
            {
                if (note.NoteType == (int)NoteType.Curve)
                {
                    loadedChartInfo.AddCurveNoteData(note.BeatBegin, note.BeatEnd, note.Lane, NoteType.Curve, note.Segments.Select(segment => new CurveSegment
                    {
                        Points = segment.Points.Select(point => new Vector2(point.X, point.Y)).ToList(),
                        StartT = segment.StartTime,
                        EndT = segment.EndTime,
                    }).ToList(), note.CurveDuration, note.UseMidPoint);
                }
                else
                {
                    loadedChartInfo.AddNoteData(note.BeatBegin, note.BeatEnd, note.Lane, (NoteType)note.NoteType);
                }
            }
            return loadedChartInfo;
        }
    }
}