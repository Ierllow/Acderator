using Cysharp.Threading.Tasks;
using Intense;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Song
{
    public class SongParticleController : MonoBehaviour, IController
    {
        [SerializeField] private ParticleObject tapParticleObject;
        [SerializeField] private ParticleObject holdParticleObject;
        [SerializeField] private ParticleObject judgeParticleObject;
        [SerializeField] private Transform particlePoolTransform;

        private ObjectPool<ParticleObject> tapParticlePool;
        private ObjectPool<ParticleObject> holdParticlePool;
        private ObjectPool<ParticleObject> judgeParticlePool;

        private readonly Dictionary<int, ParticleObject> playingHoldParticleDict = new();

        private void Awake()
        {
            tapParticlePool = CreatePool(tapParticleObject);
            holdParticlePool = CreatePool(holdParticleObject);
            judgeParticlePool = CreatePool(judgeParticleObject);
        }

        private ObjectPool<ParticleObject> CreatePool(ParticleObject prefab) => new(
            createFunc: () => Instantiate(prefab, particlePoolTransform),
            actionOnGet: p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: 4,
            maxSize: 10
        );

        public void UpdateParticles(FingerInfo fingerInfo)
        {
            if (fingerInfo.IsMissed) StopHoldEffect(fingerInfo.NoteData.Lane);
            else UpdateParticles((fingerInfo.FingerType, fingerInfo.NoteData.NoteType, fingerInfo.JudgmentType, fingerInfo.NoteData.Lane, fingerInfo.TappingLanes));
        }

        private void UpdateParticles((EFingerType, ENoteType, EJudgementType, int, IReadOnlyList<int>) particleInfo)
        {
            if (particleInfo.Item3 == EJudgementType.None) return;

            var particlePosition = particleInfo.Item4 switch
            {
                0 => 1.77f,
                1 => 3.7f,
                2 => 5.7f,
                3 => 7.75f,
                _ => default,
            };
            var judgeParticlePosition = particleInfo.Item4 switch
            {
                0 => 0.21f,
                1 => 0.09f,
                2 => -0.07f,
                3 => -0.25f,
                _ => default,
            };

            SpawnJudgeEffect(particlePosition, judgeParticlePosition, particleInfo.Item3).Forget();
            if (particleInfo.Item2 == ENoteType.Long || particleInfo.Item2 == ENoteType.Curve)
            {
                if (particleInfo.Item1 != EFingerType.Up)
                    PlayHoldEffect(particleInfo, particlePosition, judgeParticlePosition).Forget();
                else
                    StopHoldEffect(particleInfo.Item4);
                return;
            }
            SpawnTapEffect(particlePosition).Forget();
        }

        private async UniTask SpawnJudgeEffect(float parentX, float childX, EJudgementType judgementType)
        {
            var judgeParticle = judgeParticlePool.Get();
            judgeParticle.Emit(parentX, childX, judgementType);
            await UniTask.WaitWhile(() => judgeParticle.IsPlaying, cancellationToken: destroyCancellationToken);
            judgeParticlePool.Release(judgeParticle);
        }

        private async UniTask SpawnTapEffect(float position)
        {
            var tapParticle = tapParticlePool.Get();
            tapParticle.Emit(position);
            await UniTask.WaitWhile(() => tapParticle.IsPlaying, cancellationToken: destroyCancellationToken);
            tapParticlePool.Release(tapParticle);
        }

        private async UniTask PlayHoldEffect((EFingerType fingerType, ENoteType noteType, EJudgementType judgeType, int lane, IReadOnlyList<int> tappingLanes) particleInfo, float parentX, float childX)
        {
            if (!playingHoldParticleDict.ContainsKey(particleInfo.lane)
                && particleInfo.fingerType == EFingerType.Down
                && particleInfo.judgeType != EJudgementType.Miss)
            {
                var holdParticle = holdParticlePool.Get();
                holdParticle.Play(parentX);
                playingHoldParticleDict.Add(particleInfo.lane, holdParticle);
            }
            if (playingHoldParticleDict.TryGetValue(particleInfo.lane, out var playing)
                && !TappingLanesContains(particleInfo.tappingLanes, particleInfo.lane))
            {
                holdParticlePool.Release(playing);
                playingHoldParticleDict.Remove(particleInfo.lane);
                await SpawnJudgeEffect(parentX, childX, particleInfo.judgeType);
            }
        }

        private static bool TappingLanesContains(IReadOnlyList<int> lanes, int lane)
        {
            if (lanes == null) return false;
            for (var i = 0; i < lanes.Count; i++)
            {
                if (lanes[i] == lane) return true;
            }
            return false;
        }

        private void StopHoldEffect(int lane)
        {
            if (playingHoldParticleDict.TryGetValue(lane, out var holdParticle))
            {
                holdParticle.Stop();
                holdParticlePool.Release(holdParticle);
                playingHoldParticleDict.Remove(lane);
            }
        }

        private void OnDestroy()
        {
            tapParticlePool.Dispose();
            holdParticlePool.Dispose();
            judgeParticlePool.Dispose();
        }
    }
}