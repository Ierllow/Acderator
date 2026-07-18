using MessagePack;
using Song;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Intense.Api
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ChartResponse : ResponseBase
    {
        [Key("tempo")] public int Tempo { get; set; }
        [Key("speed_changes")] public List<ChartSpeedChange> SpeedChanges { get; set; } = new();
        [Key("notes")] public List<ChartNote> Notes { get; set; } = new();

        public LoadedChartInfo ToLoadedChartInfo()
        {
            var loadedChartInfo = new LoadedChartInfo
            {
                HeaderData = new HeaderData
                {
                    Tempo = Tempo,
                    NoteSpeedChangeList = SpeedChanges.Select(s => new NoteSpeedChange { Beat = s.Beat, Speed = s.Speed }).ToList(),
                },
            };

            foreach (var note in Notes)
            {
                if (note.NoteType == (int)ENoteType.Curve)
                {
                    var segments = note.Segments.Select(segment => new CurveSegment
                    {
                        Points = segment.Points.Select(p => new Vector2(p.X, p.Y)).ToList(),
                        StartT = segment.StartTime,
                        EndT = segment.EndTime,
                    }).ToList();
                    loadedChartInfo.AddCurveNoteData(note.BeatBegin, note.BeatEnd, note.Lane, ENoteType.Curve, segments, note.CurveDuration, note.UseMidPoint);
                }
                else
                {
                    loadedChartInfo.AddNoteData(note.BeatBegin, note.BeatEnd, note.Lane, (ENoteType)note.NoteType);
                }
            }

            return loadedChartInfo;
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class ChartSpeedChange
    {
        [Key("beat")] public float Beat { get; set; }
        [Key("speed")] public double Speed { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class ChartNote
    {
        [Key("beat_begin")] public float BeatBegin { get; set; }
        [Key("beat_end")] public float BeatEnd { get; set; }
        [Key("lane")] public int Lane { get; set; }
        [Key("note_type")] public int NoteType { get; set; }
        [Key("curve_duration")] public float CurveDuration { get; set; }
        [Key("use_mid_point")] public bool UseMidPoint { get; set; }
        [Key("segments")] public List<ChartCurveSegment> Segments { get; set; } = new();
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class ChartCurveSegment
    {
        [Key("start_time")] public float StartTime { get; set; }
        [Key("end_time")] public float EndTime { get; set; }
        [Key("points")] public List<ChartVector2> Points { get; set; } = new();
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class ChartVector2
    {
        [Key("x")] public float X { get; set; }
        [Key("y")] public float Y { get; set; }
    }
}