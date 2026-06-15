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

        private List<Vector2> curvePointList = new();
        private float curveDuration;
        private float secBegin;
        private bool useMidPoint;

        public override void Init(NoteData data)
        {
            base.Init(data);
            curvePointList = data.CurvePointList;
            curveDuration = data.CurveDuration;
            secBegin = data.SecBegin;
            useMidPoint = data.UseMidPoint;

            if (curvePointList.Count >= 2)
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
            if (curvePointList.Count < 2) return;

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
            if (curvePointList.Count < 2) return Vector2.zero;

            var p0 = curvePointList[0];
            var p1 = curvePointList[1];

            if (curvePointList.Count == 2) return Vector2.Lerp(p0, p1, t);

            var p2 = curvePointList.Count > 2 ? curvePointList[2] : p1;
            if (curvePointList.Count == 3) return QuadraticBezier(p0, p1, p2, t);

            var p3 = curvePointList.Count > 3 ? curvePointList[3] : p2;
            return CubicBezier(p0, p1, p2, p3, t);
        }

        private Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            var u = 1 - t;
            return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }

        private Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            var u = 1 - t;
            return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        private void UpdateCurvePosition(float yOffset)
        {
            if (curvePointList.Count < 2) return;

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