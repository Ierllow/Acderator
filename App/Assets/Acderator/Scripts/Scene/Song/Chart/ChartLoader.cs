#nullable enable

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Song
{
    public class ChartLoader
    {
        private enum ChartDataType { None, Unknown, SingleNote, LongNote, CurveNote }

        private float beat;

        private const float CurveStartY = 10f;
        private const float CurveEndY = 4f;
        private const float LaneWidth = 2f;
        private const int LaneStep = 4;

        private class ParsedLine
        {
            public int MeasureNumber { get; init; }
            public string Type { get; init; } = string.Empty;
            public ChartDataType DataType { get; init; }
            public int Lane { get; init; }
            public int LaneValue { get; init; }
            public string Identifier { get; init; } = string.Empty;
            public string Body { get; init; } = string.Empty;
        }

        private class CurvePoint
        {
            public float Beat { get; init; }
            public int LaneValue { get; init; }
            public char Kind { get; init; }
            public string Identifier { get; init; } = string.Empty;
        }

        public async UniTask LoadChart(string target, LoadedChartInfo loadedChartInfo)
        {
            if (string.IsNullOrEmpty(target))
            {
                loadedChartInfo.LoadResult = ELoadResult.Unknown;
                return;
            }

            var completionSource = AutoResetUniTaskCompletionSource.Create();
            try
            {
                var lineList = target.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                LoadHeaderData(loadedChartInfo, lineList);
                LoadMainData(loadedChartInfo, lineList);
                ProcessLongNotes(loadedChartInfo, lineList);
                ProcessCurveNotes(loadedChartInfo, lineList);

                if (loadedChartInfo.NoteDataList.Count == 0)
                {
                    loadedChartInfo.LoadResult = ELoadResult.Unknown;
                }
                completionSource.TrySetResult();
            }
            catch
            {
                loadedChartInfo.LoadResult = ELoadResult.Exception;
                completionSource.TrySetResult();
            }
            await completionSource.Task;
        }

        private void LoadHeaderData(LoadedChartInfo loadedChartInfo, List<string> lineList)
        {
            var match = lineList.Select(line => Regex.Match(line, @"#([0-9]{3}02):(.*)")).FirstOrDefault(m => m.Success);
            beat = match?.Success == true ? (float)Convert.ToDouble(match.Groups[2].Value) : 4;

            match = lineList.Select(line => Regex.Match(line, @"#(BPM01):(.*)")).FirstOrDefault(m => m.Success);
            var tempo = match?.Success == true ? Convert.ToInt32(match.Groups[2].Value) : 120;

            var speedChangeList = new List<NoteSpeedChange>();
            foreach (var line in lineList.Where(l => l.StartsWith("#TIL00")))
            {
                match = Regex.Match(line, @"#(TIL00):(.*)");
                if (!match.Success) continue;

                var tilData = match.Groups[2].Value;
                if (string.IsNullOrEmpty(tilData)) continue;

                var changeList = tilData.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(rawText =>
                {
                    var normalizedText = rawText.Replace(":", ",").Replace("\\", "").Replace("'", ",");
                    var parts = normalizedText.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length < 3) return null;

                    var measure = Convert.ToInt32(parts[0]);
                    var tick = Convert.ToInt32(parts[1]);
                    var totalBeat = measure * beat + tick / 192f * beat;
                    return new NoteSpeedChange { Beat = (float)Math.Round(totalBeat * (60f / tempo), 2, MidpointRounding.AwayFromZero), Speed = Convert.ToDouble(parts[2]) };
                }).OfType<NoteSpeedChange>().ToList();

                speedChangeList.AddRange(changeList);
            }
            loadedChartInfo.HeaderData = new() { NoteSpeedChangeList = speedChangeList, Tempo = tempo };
        }

        private List<CurvePoint> CollectPoints(List<string> lineList, ChartDataType dataType)
        {
            var points = new List<CurvePoint>();
            foreach (var line in lineList.Where(l => !l.StartsWith("#0000")))
            {
                var parsedLine = ParseLine(line);
                if (parsedLine.DataType != dataType) continue;

                var countObj = parsedLine.Body.Length / 2;
                for (var i = 0; i < countObj; i++)
                {
                    var objNum = parsedLine.Body.Substring(i * 2, 2);
                    if (objNum == "00") continue;

                    points.Add(new()
                    {
                        Beat = CalculateBeat(parsedLine.MeasureNumber, i, countObj),
                        LaneValue = parsedLine.LaneValue,
                        Kind = objNum[0],
                        Identifier = parsedLine.Identifier,
                    });
                }
            }
            return points;
        }

        private void ProcessLongNotes(LoadedChartInfo loadedChartInfo, List<string> lineList)
        {
            foreach (var group in CollectPoints(lineList, ChartDataType.LongNote).GroupBy(p => (p.LaneValue, p.Identifier)))
            {
                float? beginBeat = null;
                foreach (var point in group.OrderBy(p => p.Beat))
                {
                    if (IsLongNoteStart(point.Kind)) beginBeat = point.Beat;
                    else if (IsLongNoteEnd(point.Kind) && beginBeat.HasValue)
                    {
                        loadedChartInfo.AddNoteData(beginBeat.Value, point.Beat, point.LaneValue / LaneStep, ENoteType.Long);
                        beginBeat = null;
                    }
                }
            }
        }

        private void LoadMainData(LoadedChartInfo loadedChartInfo, List<string> lineList)
        {
            foreach (var line in lineList.Where(l => !l.StartsWith("#0000")))
            {
                var parsedLine = ParseLine(line);
                if (parsedLine.DataType is ChartDataType.None or ChartDataType.Unknown) continue;

                ProcessMainDataLine(loadedChartInfo, parsedLine);
            }
        }

        private ParsedLine ParseLine(string line)
        {
            var match = Regex.Match(line, @"#([0-9]{3})([0-9A-Za-z]{2})(.*):\s*(.*)");
            if (!match.Success) return new ParsedLine { MeasureNumber = 0, Type = "", DataType = ChartDataType.None, Lane = 0, Body = "" };

            var measureNum = Convert.ToInt32(match.Groups.ElementAtOrDefault(1).Value);
            var type = match.Groups.ElementAtOrDefault(2).Value;
            var identifier = match.Groups.ElementAtOrDefault(3).Value;
            var body = match.Groups.ElementAtOrDefault(4).Value;

            var laneValue = HexValue(type.ElementAtOrDefault(1));
            return new()
            {
                MeasureNumber = measureNum,
                Type = type,
                DataType = type.FirstOrDefault() switch
                {
                    '1' => ChartDataType.SingleNote,
                    '2' => ChartDataType.LongNote,
                    '3' => ChartDataType.CurveNote,
                    _ => ChartDataType.Unknown,
                },
                Lane = laneValue / 4,
                LaneValue = laneValue,
                Identifier = identifier,
                Body = body
            };
        }

        private void ProcessMainDataLine(LoadedChartInfo loadedChartInfo, ParsedLine parsedLine)
        {
            var countObj = parsedLine.Body.Length / 2;
            for (var i = 0; i < countObj; i++)
            {
                var objNum = parsedLine.Body.Substring(i * 2, 2);
                if (objNum == "00") continue;

                var beat = CalculateBeat(parsedLine.MeasureNumber, i, countObj);
                var objNumFirst = objNum[0];
                switch (parsedLine.DataType)
                {
                    case ChartDataType.SingleNote when objNumFirst == '1':
                        loadedChartInfo.AddNoteData(beat, beat, parsedLine.Lane, ENoteType.Single);
                        break;
                    case ChartDataType.SingleNote when objNumFirst == '3':
                        loadedChartInfo.AddNoteData(beat, beat, parsedLine.Lane, ENoteType.Flick);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ProcessCurveNotes(LoadedChartInfo loadedChartInfo, List<string> lineList)
        {
            foreach (var group in CollectPoints(lineList, ChartDataType.CurveNote).GroupBy(p => p.Identifier))
            {
                var slide = new List<CurvePoint>();
                foreach (var point in group.OrderBy(p => p.Beat))
                {
                    slide.Add(point);
                    if (!IsCurveNoteEnd(point.Kind)) continue;

                    EmitCurveNote(loadedChartInfo, slide);
                    slide = new();
                }
                EmitCurveNote(loadedChartInfo, slide);
            }
        }

        private void EmitCurveNote(LoadedChartInfo loadedChartInfo, List<CurvePoint> points)
        {
            if (points.Count < 2) return;

            var beatBegin = points[0].Beat;
            var beatEnd = points[^1].Beat;
            var span = beatEnd - beatBegin;
            var startLaneCoord = points[0].LaneValue / (float)LaneStep;

            Vector2 ToLocal(CurvePoint p)
            {
                var x = (p.LaneValue / (float)LaneStep - startLaneCoord) * LaneWidth;
                var t = span > 0 ? (p.Beat - beatBegin) / span : 0f;
                return new Vector2(x, Mathf.Lerp(CurveStartY, CurveEndY, t));
            }
            float ToTime(CurvePoint p) => span > 0 ? (p.Beat - beatBegin) / span : 0f;

            var segments = new List<CurveSegment>();
            var segPoints = new List<Vector2> { ToLocal(points[0]) };
            var segStartT = ToTime(points[0]);
            for (var i = 1; i < points.Count; i++)
            {
                segPoints.Add(ToLocal(points[i]));
                var isAnchor = !IsCurveControlPoint(points[i].Kind) || i == points.Count - 1;
                if (!isAnchor) continue;

                segments.Add(new() { Points = segPoints, StartT = segStartT, EndT = ToTime(points[i]) });
                segStartT = ToTime(points[i]);
                segPoints = new() { ToLocal(points[i]) };
            }
            if (segments.Count == 0) return;

            var lane = points[0].LaneValue / LaneStep;
            var curveDuration = span / (loadedChartInfo.HeaderData.Tempo / 60f);
            var useMidPoint = segments.Count > 1 || segments[0].Points.Count > 2;
            loadedChartInfo.AddCurveNoteData(beatBegin, beatEnd, lane, ENoteType.Curve, segments, curveDuration, useMidPoint);
        }

        private float CalculateBeat(int measureNum, int index, int countObj) => measureNum * beat + index * beat / countObj;
        private bool IsLongNoteStart(char c) => c == '1';
        private bool IsLongNoteEnd(char c) => c == '2';
        private bool IsCurveNoteEnd(char c) => c == '2';
        private bool IsCurveControlPoint(char c) => c == '4';

        private int HexValue(char c) => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => c - 'a' + 10,
            >= 'A' and <= 'F' => c - 'A' + 10,
            _ => 0,
        };
    }
}