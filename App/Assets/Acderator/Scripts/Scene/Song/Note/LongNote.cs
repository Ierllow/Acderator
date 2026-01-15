using Intense;
using System;
using UnityEngine;
using Zenject;

namespace Song
{
    public class LongNote : NoteBase
    {
        [SerializeField] private SpriteRenderer beginSprite;
        [SerializeField] private SpriteRenderer trailSprite;
        [SerializeField] private SpriteRenderer endSprite;
        [SerializeField] private Color32 defaultTrailColor; //
        [SerializeField] private Color32 tappingTrailColor;

        public override void MoveNote(float positionBeginY, float positionEndY, float currentNoteSpeed)
        {
            transform.localPosition = new(0, positionBeginY, 0);
            var length = positionEndY - positionBeginY;
            beginSprite.transform.localPosition = Vector3.zero;
            endSprite.transform.localPosition = new(0, length, 0);
            trailSprite.transform.localPosition = new(0, length / 2f, 0);
            trailSprite.transform.localScale = new(trailSprite.transform.localScale.x, length, 1f);
        }

        public override void OnJudgedNote(EFingerType fingerType, EJudgementType judgmentType)
        {
            switch (fingerType)
            {
                case EFingerType.Down when !judgmentType.EnumEquals(EJudgementType.None):
                    IsTapping = true;
                    trailSprite.color = tappingTrailColor;
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