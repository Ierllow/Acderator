#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Song
{
    public readonly struct CurveNoteLayout
    {
        public Vector2 Start { get; init; }
        public Vector2 Middle { get; init; }
        public Vector2 End { get; init; }

    }

    public sealed class CurveNoteModel
    {
        private readonly IReadOnlyList<CurveSegment> segments;
        private readonly Vector2[] evaluationBuffer;
        private readonly float startSec;
        private readonly float duration;

        public bool IsEmpty => segments.Count == 0;
        public bool UsesMidPoint { get; }

        public CurveNoteModel(NoteData data)
        {
            segments = data.CurveSegmentList;
            startSec = data.SecBegin;
            duration = data.CurveDuration;
            UsesMidPoint = data.UseMidPoint;

            var maxPointCount = 0;
            foreach (var segment in segments)
            {
                maxPointCount = Mathf.Max(maxPointCount, segment.Points.Count);
            }
            evaluationBuffer = maxPointCount > 0 ? new Vector2[maxPointCount] : Array.Empty<Vector2>();
        }

        public CurveNoteLayout GetLayout(float currentSec)
        {
            var progress = duration > 0 ? Mathf.Clamp01((currentSec - startSec) / duration) : 1f;
            return new()
            {
                Start = Evaluate(progress),
                Middle = Evaluate(Mathf.Lerp(progress, 1f, 0.5f)),
                End = Evaluate(1f)
            };
        }

        public Vector2 Evaluate(float progress)
        {
            if (IsEmpty) return Vector2.zero;

            var segment = segments[^1];
            foreach (var candidate in segments)
            {
                if (progress > candidate.EndT) continue;
                segment = candidate;
                break;
            }

            var span = segment.EndT - segment.StartT;
            var localProgress = span > 0 ? Mathf.Clamp01((progress - segment.StartT) / span) : 1f;
            return EvaluateBezier(segment.Points, localProgress);
        }

        private Vector2 EvaluateBezier(IReadOnlyList<Vector2> points, float progress)
        {
            var count = points.Count;
            if (count == 0) return Vector2.zero;
            if (count == 1) return points[0];
            if (count == 2) return Vector2.Lerp(points[0], points[1], progress);

            for (var i = 0; i < count; i++) evaluationBuffer[i] = points[i];
            for (var step = 1; step < count; step++)
            {
                for (var i = 0; i < count - step; i++)
                {
                    evaluationBuffer[i] = Vector2.Lerp(evaluationBuffer[i], evaluationBuffer[i + 1], progress);
                }
            }
            return evaluationBuffer[0];
        }
    }
}