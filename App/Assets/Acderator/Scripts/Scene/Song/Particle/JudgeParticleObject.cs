using System;
using Intense;
using Intense.UI;
using UnityEngine;

namespace Song
{
    public class JudgeParticleObject : ParticleObject
    {
        [SerializeField] private AtlasImage atlas;
        [SerializeField] private ParticleSystemRenderer particleSystemRenderer;

        public void Emit(float xParentPosition, float xChildPosition, EJudgementType type)
        {
            if (type.EnumEquals(EJudgementType.None)) return;

            atlas.SetAtlasFormat("judgetext_{0}", (int)type, "song/particle/judgetext");
            particleSystemRenderer.material.mainTexture = atlas.mainTexture;
            particle.transform.localPosition = new(xChildPosition, particle.transform.localPosition.y, particle.transform.localPosition.z);
            base.Emit(xParentPosition);
        }
    }
}