#nullable enable

using Intense;
using System;
using UnityEngine;

namespace Song
{
    public abstract class NoteBase : MonoBehaviour
    {
        public bool IsTapping { get; protected set; } = false;

        public bool IsActive => gameObject.activeSelf;

        public Action<NoteBase>? Finalized;

        private void OnDisable() => IsTapping = false;

        public void Release() => Finalized?.Invoke(this);

        public virtual void Init(NoteData data) => transform.localPosition = new Vector3(0, 50, 0);

        public virtual void UpdatePosition(NotePositionUpdateContext context) => transform.localPosition = new(0, context.PositionBeginY, 0);

        public abstract void OnJudgedNote(FingerType fingerType, JudgementType judgmentType = JudgementType.None);

        protected bool TryBeginTap(FingerType fingerType, JudgementType judgmentType)
        {
            if (fingerType != FingerType.Down || judgmentType == JudgementType.None) return false;

            IsTapping = true;
            return true;
        }
    }
}