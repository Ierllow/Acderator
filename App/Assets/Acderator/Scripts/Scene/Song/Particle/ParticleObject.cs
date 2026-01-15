using UnityEngine;

namespace Song
{
    public class ParticleObject : MonoBehaviour
    {
        protected ParticleSystem particle;

        public virtual bool IsPlaying => particle.isPlaying;

        protected virtual void Awake() => particle = GetComponent<ParticleSystem>();

        public virtual void Emit(float xPosition)
        {
            transform.localPosition = new Vector3(xPosition, transform.localPosition.y, transform.localPosition.z);
            particle.Clear();
            particle.Emit(1);
        }

        public virtual void Stop() => particle.Stop();
    }
}