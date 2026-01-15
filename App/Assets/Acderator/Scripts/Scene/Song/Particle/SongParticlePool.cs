using Intense;
using Zenject;

namespace Song
{
    public class TapParticlePool : MonoMemoryPool<float, ParticleObject>
    {
        protected override void Reinitialize(float xPosition, ParticleObject item) => item.Emit(xPosition);
    }

    public class HoldParticlePool : MonoMemoryPool<float, HoldParticleObject>
    {
        protected override void Reinitialize(float xPosition, HoldParticleObject item) => item.Play(xPosition);
    }

    public class JudgeParticlePool : MonoMemoryPool<float, float, EJudgementType, JudgeParticleObject>
    {
        protected override void Reinitialize(float xParentPos, float xChildPos, EJudgementType type, JudgeParticleObject item) => item.Emit(xParentPos, xChildPos, type);
    }
}