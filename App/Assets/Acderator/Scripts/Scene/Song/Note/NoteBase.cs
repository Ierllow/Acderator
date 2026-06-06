using Intense;
using System;
using UnityEngine;

namespace Song
{
    public abstract class NoteBase : MonoBehaviour
    {
        public bool IsTapping { get; protected set; } = false;

        public bool IsActive => gameObject.activeSelf;

        public Action<NoteBase> Finalized;

        private void OnDisable()
        {
            IsTapping = false;
            Finalized?.Invoke(this);
        }

        public virtual void Init(NoteData data) => transform.localPosition = new Vector3(0, 50, 0);

        public virtual void UpdatePosition(NotePositionUpdateContext context) => transform.localPosition = new(0, context.PositionBeginY, 0);

        public abstract void OnJudgedNote(EFingerType fingerType, EJudgementType judgmentType = EJudgementType.None);
    }
}