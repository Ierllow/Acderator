using Intense;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ZLinq;

namespace Song
{
    public class CurveNote : NoteBase
    {
        [SerializeField] private SpriteRenderer beginSprite;
        [SerializeField] private SpriteRenderer trailSprite;
        [SerializeField] private SpriteRenderer endSprite;
        [SerializeField] private LineRenderer curveLineRenderer;
        [SerializeField] private Color32 tappingTrailColor;
        [SerializeField] private int curveSegments = 21;

        private List<Vector2> curvePointList;
        private float curveDuration;

        public new void OnSpawned(NoteData data, IMemoryPool pool)
        {
            base.OnSpawned(data, pool);

            NoteData = data;
            this.pool = pool;
            curvePointList = data.CurvePointList;
            curveDuration = data.CurveDuration;

            if (curveLineRenderer != default && curvePointList.AsValueEnumerable().Count() >= 2)
            {
                curveLineRenderer.positionCount = curveSegments + 1;
                curveLineRenderer.useWorldSpace = false;
                UpdateCurvePosition(0);
            }

            if (!data.UseMidPoint)
            {
                if (endSprite != default) endSprite.gameObject.SetActive(false);
                if (trailSprite != default) trailSprite.gameObject.SetActive(false);
                return;
            }
            if (endSprite != default) endSprite.gameObject.SetActive(true);
            if (trailSprite != default) trailSprite.gameObject.SetActive(true);
        }

        public void MoveNote(float positionBeginY, float positionEndY, float currentNoteSpeed, float currentSec)
        {
            if (curvePointList.Count < 2) return;

            transform.localPosition = new(0, positionBeginY, transform.localPosition.z);
            var curveProgress = Mathf.Clamp01((currentSec - NoteData.SecBegin) / curveDuration);
            var startCurvePos = GetCurvePosition(curveProgress);
            beginSprite.transform.localPosition = new(startCurvePos.x, startCurvePos.y, 0);

            if (NoteData.UseMidPoint)
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
            if (curveLineRenderer == default || curvePointList.Count < 2) return;

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
                case EFingerType.Down when !judgmentType.EnumEquals(EJudgementType.None):
                    IsTapping = true;
                    if (trailSprite != default) trailSprite.color = tappingTrailColor;
                    break;
                case EFingerType.Up:
                    Final();
                    break;
                default:
                    break;
            }
        }
    }
}