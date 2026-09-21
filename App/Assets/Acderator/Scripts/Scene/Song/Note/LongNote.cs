#nullable enable

using UnityEngine;

namespace Song
{
    public class LongNote : HoldNoteBase
    {
        [SerializeField] private SpriteRenderer beginSprite = default!;
        [SerializeField] private SpriteRenderer endSprite = default!;

        public override void UpdatePosition(NotePositionUpdateContext context)
        {
            transform.localPosition = new(0, context.PositionBeginY, 0);
            var length = context.PositionEndY - context.PositionBeginY;
            beginSprite.transform.localPosition = Vector3.zero;
            endSprite.transform.localPosition = new(0, length, 0);
            trailSprite.transform.localPosition = new(0, length / 2f, 0);
            trailSprite.transform.localScale = new(trailSprite.transform.localScale.x, length, 1f);
        }
    }
}