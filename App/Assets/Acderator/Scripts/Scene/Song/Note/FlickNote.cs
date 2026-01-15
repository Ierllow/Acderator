using DG.Tweening;
using Intense;
using System;
using UnityEngine;

namespace Song
{
    public class FlickNote : NoteBase
    {
        [SerializeField] private SpriteRenderer arrow;

        private void Awake()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(arrow.transform.DOLocalMoveZ(arrow.transform.localPosition.z + -0.2f, 0.2f));
            sequence.OnComplete(() => arrow.transform.localPosition = new Vector3(arrow.transform.localPosition.x, arrow.transform.localPosition.y, arrow.transform.localPosition.z + 0.2f));
            sequence.SetEase(Ease.Flash, 1).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
            sequence.Play();
        }

        public override void OnJudgedNote(EFingerType fingerType, EJudgementType judgmentType = EJudgementType.None)
        {
            switch (fingerType)
            {
                case EFingerType.Down:
                    IsTapping = !judgmentType.EnumEquals(EJudgementType.None);
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