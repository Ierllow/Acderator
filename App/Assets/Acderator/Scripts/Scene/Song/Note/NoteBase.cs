using Intense;
using UnityEngine;

namespace Song
{
    public abstract class NoteBase : MonoBehaviour
    {
        public NoteData NoteData { get; protected set; }
        public bool IsTapping { get; protected set; } = false;

        public bool IsActive => gameObject.activeSelf;

        protected NotePool<NoteBase> pool;

        public void Init(NoteData data, NotePool<NoteBase> pool)
        {
            this.pool = pool;
            NoteData = data;
            transform.localPosition = new Vector3(0, 50, 0);
        }

        public virtual void MoveNote(float positionBeginY, float positionEndY, float currentNoteSpeed) => transform.localPosition = new(0, positionBeginY, 0);

        public abstract void OnJudgedNote(EFingerType fingerType, EJudgementType judgmentType = EJudgementType.None);

        public virtual void Final()
        {
            pool.Release(this);
            pool = default;
            IsTapping = false;
        }
    }
}