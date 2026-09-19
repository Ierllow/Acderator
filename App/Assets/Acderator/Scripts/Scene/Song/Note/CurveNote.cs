#nullable enable

using Intense;
using System.Collections.Generic;
using UnityEngine;

namespace Song
{
    public class CurveNote : NoteBase
    {
        [SerializeField] private SpriteRenderer beginSprite = default!;
        [SerializeField] private SpriteRenderer trailSprite = default!;
        [SerializeField] private SpriteRenderer endSprite = default!;
        [SerializeField] private LineRenderer curveLineRenderer = default!;
        [SerializeField] private Color32 tappingTrailColor;
        [SerializeField] private int curveSegments = 21;

        private List<CurveSegment> segmentList = new();
        private Vector2[] curveBuffer = System.Array.Empty<Vector2>();
        private float curveDuration;
        private float secBegin;
        private bool useMidPoint;
        private Color initialTrailColor;

        private void Awake() => initialTrailColor = trailSprite.color;

        public override void Init(NoteData data)
        {
            base.Init(data);
            trailSprite.color = initialTrailColor;
            segmentList = data.CurveSegmentList;
            var maxPoints = 0;
            foreach (var segment in segmentList) maxPoints = Mathf.Max(maxPoints, segment.Points.Count);
            curveBuffer = new Vector2[maxPoints];
            curveDuration = data.CurveDuration;
            secBegin = data.SecBegin;
            useMidPoint = data.UseMidPoint;

            if (segmentList.Count > 0)
            {
                curveLineRenderer.positionCount = curveSegments + 1;
                curveLineRenderer.useWorldSpace = false;
                UpdateCurvePosition(0);
            }

            if (!data.UseMidPoint)
            {
                endSprite.gameObject.SetActive(false);
                trailSprite.gameObject.SetActive(false);
                return;
            }
            endSprite.gameObject.SetActive(true);
            trailSprite.gameObject.SetActive(true);
        }

        public override void UpdatePosition(NotePositionUpdateContext context)
        {
            if (segmentList.Count == 0) return;

            transform.localPosition = new(0, context.PositionBeginY, transform.localPosition.z);
            var curveProgress = curveDuration > 0 ? Mathf.Clamp01((context.CurrentSec - secBegin) / curveDuration) : 1f;
            var startCurvePos = GetCurvePosition(curveProgress);
            beginSprite.transform.localPosition = new(startCurvePos.x, startCurvePos.y, 0);

            if (useMidPoint)
            {
                var endCurvePosition = GetCurvePosition(1.0f);
                endSprite.transform.localPosition = new(endCurvePosition.x, endCurvePosition.y, 0);
                var midCurvePosition = GetCurvePosition(Mathf.Lerp(curveProgress, 1.0f, 0.5f));
                trailSprite.transform.localPosition = new(midCurvePosition.x, midCurvePosition.y, 0);
                var scale = trailSprite.transform.localScale;
                scale.y = Vector2.Distance(startCurvePos, endCurvePosition);
                trailSprite.transform.localScale = scale;
            }
            UpdateCurvePosition(0);
        }

        private Vector2 GetCurvePosition(float t)
        {
            if (segmentList.Count == 0) return Vector2.zero;

            var segment = segmentList[^1];
            foreach (var candidate in segmentList)
            {
                if (t > candidate.EndT) continue;
                segment = candidate;
                break;
            }

            var span = segment.EndT - segment.StartT;
            var localT = span > 0 ? Mathf.Clamp01((t - segment.StartT) / span) : 1f;
            return EvaluateBezier(segment.Points, localT);
        }

        private Vector2 EvaluateBezier(List<Vector2> points, float t)
        {
            var count = points.Count;
            if (count == 0) return Vector2.zero;
            if (count == 1) return points[0];
            if (count == 2) return Vector2.Lerp(points[0], points[1], t);

            for (var i = 0; i < count; i++) curveBuffer[i] = points[i];
            for (var step = 1; step < count; step++)
            {
                for (var i = 0; i < count - step; i++)
                {
                    curveBuffer[i] = Vector2.Lerp(curveBuffer[i], curveBuffer[i + 1], t);
                }
            }
            return curveBuffer[0];
        }

        private void UpdateCurvePosition(float yOffset)
        {
            if (segmentList.Count == 0) return;

            for (var i = 0; i <= curveSegments; i++)
            {
                var t = i / (float)curveSegments;
                var curvePosition = GetCurvePosition(t);
                curveLineRenderer.SetPosition(i, new Vector3(curvePosition.x, curvePosition.y + yOffset, 0));
            }
        }

        public override void OnJudgedNote(EFingerType fingerType, EJudgementType judgmentType)
        {
            switch (fingerType)
            {
                case EFingerType.Down when judgmentType != EJudgementType.None:
                    IsTapping = true;
                    trailSprite.color = tappingTrailColor;
                    break;
                default:
                    break;
            }
        }
    }
}