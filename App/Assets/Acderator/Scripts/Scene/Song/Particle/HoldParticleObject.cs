using UnityEngine;

namespace Song
{
    public class HoldParticleObject : ParticleObject
    {
        public void Play(float xPosition)
        {
            transform.localPosition = new Vector3(xPosition, transform.localPosition.y, transform.localPosition.z);
            particle.Clear();
            particle.Play();
        }
    }
}