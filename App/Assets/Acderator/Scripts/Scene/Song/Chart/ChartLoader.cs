using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using ZLinq;

namespace Song
{
    public class ChartLoader
    {
        private enum ChartDataType { None, Unknown, SingleNote, LongNote, CurveNote }

        private readonly Dictionary<int, float> longNoteBeginDict = new();
        private float beat;

        private class ParsedLine
        {
            public int MeasureNumber { get; init; }
            public string Type { get; init; }
            public ChartDataType DataType { get; init; }
            public int Lane { get; init; }
            public string Body { get; init; }
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
                ProcessLongNoteBegins(lineList);
                LoadMainData(loadedChartInfo, lineList);

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

            longNoteBeginDict.Clear();
        }

        private void LoadHeaderData(LoadedChartInfo loadedChartInfo, List<string> lineList)
        {
            var match = lineList.AsValueEnumerable().Select(line => Regex.Match(line, @"#(...02):(.*)")).FirstOrDefault(m => m.Success);
            beat = match?.Success == true ? Convert.ToInt32(match.Groups[2].Value) : 4;

            match = lineList.AsValueEnumerable().Select(line => Regex.Match(line, @"#(BPM01):(.*)")).FirstOrDefault(m => m.Success);
            var tempo = match?.Success == true ? Convert.ToInt32(match.Groups[2].Value) : 120;

            var speedChangeList = new List<NoteSpeedChange>();
            foreach (var line in lineList.AsValueEnumerable().Where(l => l.StartsWith("#TIL00")))
            {
                match = Regex.Match(line, @"#(TIL00):(.*)");
                if (!match.Success) continue;

                var tilData = match.Groups[2].Value;
                if (string.IsNullOrEmpty(tilData)) continue;

                var changeList = tilData.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsValueEnumerable().Select(rawText =>
                {
                    var normalizedText = rawText.Replace(":", ",").Replace("\\", "").Replace("'", ",");
                    var parts = normalizedText.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length < 3) return default(NoteSpeedChange);

                    var measure = Convert.ToInt32(parts[0]);
                    var tick = Convert.ToInt32(parts[1]);
                    var totalBeat = (measure - 1) * beat + tick / 192f * beat;
                    return new() { Beat = (float)Math.Round(totalBeat * (60f / tempo), 2, MidpointRounding.AwayFromZero), Speed = Convert.ToDouble(parts[2]) };
                }).Where(c => c != null).ToList();

                speedChangeList.AddRange(changeList);
            }
            loadedChartInfo.HeaderData = new() { NoteSpeedChangeList = speedChangeList, Tempo = tempo };
        }

        private void ProcessLongNoteBegins(List<string> lineList)
        {
            foreach (var line in lineList.AsValueEnumerable().Where(l => !l.StartsWith("#0000")))
            {
                var parsedLine = ParseLine(line);
                if (!parsedLine.DataType.EnumEquals(ChartDataType.LongNote)) continue;

                var countObj = parsedLine.Body.Length / 2;
                for (var i = 0; i < countObj; i++)
                {
                    var objNum = parsedLine.Body.Substring(i * 2, 2);
                    var beat = CalculateBeat(parsedLine.MeasureNumber, i, countObj);

                    if (IsLongNoteStart(objNum.AsValueEnumerable().ElementAtOrDefault(0)))
                    {
                        longNoteBeginDict[parsedLine.Lane] = beat;
                    }
                }
            }
        }

        private void LoadMainData(LoadedChartInfo loadedChartInfo, List<string> lineList)
        {
            foreach (var line in lineList.AsValueEnumerable().Where(l => !l.StartsWith("#0000")))
            {
                var parsedLine = ParseLine(line);
                if (parsedLine.DataType.EnumEquals(ChartDataType.Unknown))
                {
                    loadedChartInfo.LoadResult = ELoadResult.Unsupported;
                    return;
                }

                ProcessMainDataLine(loadedChartInfo, parsedLine);
            }
        }

        private ParsedLine ParseLine(string line)
        {
            var match = Regex.Match(line, @"#([0-9]{3})([0-9A-Za-z]{2})(.*): (.*)");
            if (!match.Success) return new ParsedLine { MeasureNumber = 0, Type = "", DataType = ChartDataType.None, Lane = 0, Body = "" };

            var measureNum = Convert.ToInt32(match.Groups.AsValueEnumerable().ElementAtOrDefault(1).Value);
            var type = match.Groups.AsValueEnumerable().ElementAtOrDefault(2).Value;
            var body = match.Groups.AsValueEnumerable().ElementAtOrDefault(4).Value;

            var dataType = type.AsValueEnumerable().FirstOrDefault() switch
            {
                '1' => ChartDataType.SingleNote,
                '2' => ChartDataType.LongNote,
                '3' => ChartDataType.CurveNote,
                _ => ChartDataType.Unknown,
            };

            var lane = type.AsValueEnumerable().ElementAtOrDefault(1) switch
            {
                '4' => 1,
                '8' => 2,
                'c' => 3,
                _ => 0,
            };

            return new() { MeasureNumber = measureNum, Type = type, DataType = dataType, Lane = lane, Body = body };
        }

        private void ProcessMainDataLine(LoadedChartInfo loadedChartInfo, ParsedLine parsedLine)
        {
            var countObj = parsedLine.Body.Length / 2;
            for (var i = 0; i < countObj; i++)
            {
                var objNum = parsedLine.Body.Substring(i * 2, 2);
                if (objNum == "00") continue;

                var beat = CalculateBeat(parsedLine.MeasureNumber, i, countObj);
                var objNumFirst = objNum.AsValueEnumerable().ElementAt(0);
                switch (parsedLine.DataType)
                {
                    case ChartDataType.SingleNote when objNumFirst == '1':
                        loadedChartInfo.AddNoteData(beat, beat, parsedLine.Lane, ENoteType.Single);
                        break;
                    case ChartDataType.SingleNote when objNumFirst == '3':
                        loadedChartInfo.AddNoteData(beat, beat, parsedLine.Lane, ENoteType.Flick);
                        break;
                    case ChartDataType.LongNote when objNumFirst == '1':
                        longNoteBeginDict[parsedLine.Lane] = beat;
                        break;
                    case ChartDataType.LongNote when objNumFirst == '2' && longNoteBeginDict.TryGetValue(parsedLine.Lane, out var beginBeat):
                        loadedChartInfo.AddNoteData(beginBeat, beat, parsedLine.Lane, ENoteType.Long);
                        longNoteBeginDict.Remove(parsedLine.Lane);
                        break;
                    case ChartDataType.CurveNote when objNumFirst == '1':
                        var baseX = parsedLine.Lane * 2.0f - 2.0f;
                        var pointList = new List<Vector2>
                        {
                            new(baseX, 10f), // Start
                            new(baseX + 1.0f, 8f), // Midpoint1
                            new(baseX - 1.0f, 6f), // Midpoint2
                            new(baseX, 4f) // End
                        };
                        loadedChartInfo.AddCurveNoteData(beat, beat, parsedLine.Lane, ENoteType.Curve, pointList, 1.0f, pointList.Count > 2);
                        break;
                    default:
                        break;
                }
            }
        }

        private float CalculateBeat(int measureNum, int index, int countObj) => (measureNum - 1) * beat + index * beat / countObj;
        private bool IsLongNoteStart(char c) => c == '1';
        // private bool IsLongNoteEnd(char c) => c == '2';
        // private bool IsLongNoteMiddle(char c) => c == '3';
        // private bool IsCurveNoteStart(char c) => c == '1';
        // private bool IsCurveNoteEnd(char c) => c == '2';
        // private bool IsCurveNoteMiddle(char c) => c == '3';
        // private bool IsCurveControlPoint(char c) => c == '4';
        // private bool IsCurveInvisibleMiddle(char c) => c == '5';
    }
}