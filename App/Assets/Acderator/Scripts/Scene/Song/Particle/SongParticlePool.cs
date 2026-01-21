using System;
using UnityEngine.Pool;

namespace Song
{
    public class TapParticlePool : ObjectPool<ParticleObject>
    {
        public TapParticlePool(Func<ParticleObject> createFunc, Action<ParticleObject> actionOnGet = null, Action<ParticleObject> actionOnRelease = null, Action<ParticleObject> actionOnDestroy = null, bool collectionCheck = false, int defaultCapacity = 4, int maxSize = 10)
            : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize) { }
    }

    public class HoldParticlePool : ObjectPool<HoldParticleObject>
    {
        public HoldParticlePool(Func<HoldParticleObject> createFunc, Action<HoldParticleObject> actionOnGet = null, Action<HoldParticleObject> actionOnRelease = null, Action<HoldParticleObject> actionOnDestroy = null, bool collectionCheck = false, int defaultCapacity = 4, int maxSize = 10)
            : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize) { }
    }

    public class JudgeParticlePool : ObjectPool<JudgeParticleObject>
    {
        public JudgeParticlePool(Func<JudgeParticleObject> createFunc, Action<JudgeParticleObject> actionOnGet = null, Action<JudgeParticleObject> actionOnRelease = null, Action<JudgeParticleObject> actionOnDestroy = null, bool collectionCheck = false, int defaultCapacity = 4, int maxSize = 10)
            : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize) { }
    }
}