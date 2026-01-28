using System;
using UnityEngine.Pool;

namespace Song
{
    public class TapParticlePool : ObjectPool<ParticleObject>
    {
        public TapParticlePool(Func<ParticleObject> createFunc, Action<ParticleObject> actionOnGet = null, Action<ParticleObject> actionOnRelease = null, Action<ParticleObject> actionOnDestroy = null)
            : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, false, 4, 10) { }
    }

    public class HoldParticlePool : ObjectPool<HoldParticleObject>
    {
        public HoldParticlePool(Func<HoldParticleObject> createFunc, Action<HoldParticleObject> actionOnGet = null, Action<HoldParticleObject> actionOnRelease = null, Action<HoldParticleObject> actionOnDestroy = null)
            : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, false, 4, 10) { }
    }

    public class JudgeParticlePool : ObjectPool<JudgeParticleObject>
    {
        public JudgeParticlePool(Func<JudgeParticleObject> createFunc, Action<JudgeParticleObject> actionOnGet = null, Action<JudgeParticleObject> actionOnRelease = null, Action<JudgeParticleObject> actionOnDestroy = null)
            : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, false, 4, 10) { }
    }
}