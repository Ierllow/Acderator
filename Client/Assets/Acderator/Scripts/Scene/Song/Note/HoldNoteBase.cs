#nullable enable

using Intense;
using UnityEngine;

namespace Song
{
    public abstract class HoldNoteBase : NoteBase
    {
        [SerializeField] protected SpriteRenderer trailSprite = default!;
        [SerializeField] private Color32 tappingTrailColor;

        private Color initialTrailColor;

        protected virtual void Awake() => initialTrailColor = trailSprite.color;

        public override void Init(NoteData data)
        {
            base.Init(data);
            trailSprite.color = initialTrailColor;
        }

        public override void OnJudgedNote(FingerType fingerType, JudgementType judgmentType)
        {
            if (!TryBeginTap(fingerType, judgmentType)) return;
            trailSprite.color = tappingTrailColor;
        }
    }
}