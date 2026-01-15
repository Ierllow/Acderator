using Intense;
using UnityEngine;
using Zenject;

namespace Song
{
    public abstract class NoteBase : MonoBehaviour, IPoolable<NoteData, IMemoryPool>
    {
        public NoteData NoteData { get; protected set; }
        public bool IsTapping { get; protected set; } = false;

        public bool IsActive => gameObject.activeSelf;

        protected IMemoryPool pool;

        public void OnSpawned(NoteData data, IMemoryPool pool)
        {
            this.pool = pool;
            NoteData = data;
            transform.localPosition = new Vector3(0, 50, 0);
        }

        public void OnDespawned()
        {
            pool = default;
            IsTapping = false;
        }

        public virtual void MoveNote(float positionBeginY, float positionEndY, float currentNoteSpeed) => transform.localPosition = new(0, positionBeginY, 0);

        public abstract void OnJudgedNote(EFingerType fingerType, EJudgementType judgmentType = EJudgementType.None);

        public virtual void Final() => pool.Despawn(this);
    }
}