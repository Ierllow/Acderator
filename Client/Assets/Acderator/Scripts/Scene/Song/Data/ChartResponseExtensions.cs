using Acderator.Shared;
using System.Linq;
using UnityEngine;

namespace Song
{
    public static class ChartResponseExtensions
    {
        public static LoadedChartInfo ToLoadedChartInfo(this ChartResponse response)
        {
            if (response.Tempo <= 0 || response.Notes == null || response.Notes.Count == 0 || response.SpeedChanges == null) return default;
            if (response.Notes.Any(note => note.NoteType == (int)NoteType.Curve && note.Segments == null)) return default;

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