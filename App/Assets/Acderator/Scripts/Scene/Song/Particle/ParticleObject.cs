using Intense;
using Intense.UI;
using UnityEngine;

namespace Song
{
    public class ParticleObject : MonoBehaviour
    {
        [SerializeField] private AtlasImage atlas;
        [SerializeField] private ParticleSystemRenderer particleSystemRenderer;

        private ParticleSystem particle;

        public bool IsPlaying => particle.isPlaying;

        private void Awake() => particle = GetComponent<ParticleSystem>();

        public void Emit(float xPosition)
        {
            transform.localPosition = new Vector3(xPosition, transform.localPosition.y, transform.localPosition.z);
            particle.Clear();
            particle.Emit(1);
        }

        public void Emit(float xParentPosition, float xChildPosition, EJudgementType type)
        {
            if (type == EJudgementType.None) return;

            atlas.SetAtlasFormat("judgetext_{0}", (int)type, "song/particle/judgetext");
            particleSystemRenderer.material.mainTexture = atlas.mainTexture;
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