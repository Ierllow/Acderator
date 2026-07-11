#nullable enable

using Intense;
using UnityEngine;

namespace Song
{
    public class ParticleObject : MonoBehaviour
    {
        [SerializeField] private ParticleSystemRenderer particleSystemRenderer = default!;

        private ParticleSystem particle = default!;

        public bool IsPlaying => particle.isPlaying;

        private void Awake() => particle = GetComponent<ParticleSystem>();

        public void SetMainTexture(Sprite sprite)
        {
            if (sprite == null) return;
            particleSystemRenderer.material.mainTexture = sprite.texture;
        }

        public void Emit(float xPosition)
        {
            transform.localPosition = new Vector3(xPosition, transform.localPosition.y, transform.localPosition.z);
            particle.Clear();
            particle.Emit(1);
        }

        public void Emit(float xParentPosition, float xChildPosition, EJudgementType type, Sprite judgeSprite)
        {
            if (type == EJudgementType.None) return;

            if (judgeSprite != null) particleSystemRenderer.material.mainTexture = judgeSprite.texture;
            particle.transform.localPosition = new(xChildPosition, particle.transform.localPosition.y, particle.transform.localPosition.z);
            Emit(xParentPosition);
        }

        public void Play(float xPosition)
        {
            transform.localPosition = new Vector3(xPosition, transform.localPosition.y, transform.localPosition.z);
            particle.Clear();
            particle.Play();
        }

        public void Stop() => particle.Stop();
    }
}