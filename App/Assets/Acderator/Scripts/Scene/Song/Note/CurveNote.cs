#nullable enable

using UnityEngine;
using UnityEngine.Serialization;

namespace Song
{
    public class CurveNote : HoldNoteBase
    {
        [SerializeField] private SpriteRenderer beginSprite = default!;
        [SerializeField] private SpriteRenderer endSprite = default!;
        [SerializeField] private LineRenderer curveLineRenderer = default!;
        [FormerlySerializedAs("curveSegments"), SerializeField, Min(1)] private int curveResolution = 21;

        private CurveNoteModel model = default!;

        public override void Init(NoteData data)
        {
            base.Init(data);
            model = new CurveNoteModel(data);

            if (!model.IsEmpty)
            {
                curveLineRenderer.positionCount = curveResolution + 1;
                curveLineRenderer.useWorldSpace = false;
                DrawCurve();
            }
            else curveLineRenderer.positionCount = 0;

            endSprite.gameObject.SetActive(model.UsesMidPoint);
            trailSprite.gameObject.SetActive(model.UsesMidPoint);
        }

        public override void UpdatePosition(NotePositionUpdateContext context)
        {
            if (model.IsEmpty) return;

            transform.localPosition = new(0, context.PositionBeginY, transform.localPosition.z);
            var layout = model.GetLayout(context.CurrentSec);
            beginSprite.transform.localPosition = new(layout.Start.x, layout.Start.y, 0);

            if (model.UsesMidPoint)
            {
                endSprite.transform.localPosition = new(layout.End.x, layout.End.y, 0);
                trailSprite.transform.localPosition = new(layout.Middle.x, layout.Middle.y, 0);
                var scale = trailSprite.transform.localScale;
                scale.y = Vector2.Distance(layout.Start, layout.End);
                trailSprite.transform.localScale = scale;
            }
        }

        private void DrawCurve()
        {
            for (var i = 0; i <= curveResolution; i++)
            {
                var progress = i / (float)curveResolution;
                var curvePosition = model.Evaluate(progress);
                curveLineRenderer.SetPosition(i, new Vector3(curvePosition.x, curvePosition.y, 0));
            }
        }
    }
}