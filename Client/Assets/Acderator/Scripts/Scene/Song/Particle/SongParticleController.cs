#nullable enable

using Cysharp.Threading.Tasks;
using Intense;
using Intense.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

namespace Song
{
    public class SongParticleController : MonoBehaviour
    {
        [SerializeField] private ParticleObject tapParticleObject = default!;
        [SerializeField] private ParticleObject holdParticleObject = default!;
        [SerializeField] private ParticleObject judgeParticleObject = default!;
        [SerializeField] private Transform particlePoolTransform = default!;

        [Inject] private readonly AddressableAssetManager addressableAssetManager = default!;
        [Inject] private readonly AddressablePrefabResolver addressablePrefabResolver = default!;

        private ObjectPool<ParticleObject>? tapParticlePool;
        private ObjectPool<ParticleObject>? holdParticlePool;
        private ObjectPool<ParticleObject>? judgeParticlePool;

        private readonly Dictionary<int, ParticleObject> playingHoldParticleDict = new();

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
            if (!EnsurePools()) return;
            if (fingerInfo.NoteData == null) return;

            if (fingerInfo.IsMissed) StopHoldEffect(fingerInfo.NoteData.Lane);
            else UpdateParticles((fingerInfo.FingerType, fingerInfo.NoteData.NoteType, fingerInfo.JudgmentType, fingerInfo.NoteData.Lane, fingerInfo.TappingLanes ?? Array.Empty<int>()));
        }

        private void UpdateParticles((FingerType, NoteType, JudgementType, int, IReadOnlyList<int>) particleInfo)
        {
            if (particleInfo.Item3 == JudgementType.None) return;

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
            if (particleInfo.Item2 == NoteType.Long || particleInfo.Item2 == NoteType.Curve)
            {
                if (particleInfo.Item1 != FingerType.Up)
                    PlayHoldEffect(particleInfo, particlePosition, judgeParticlePosition).Forget();
                else
                    StopHoldEffect(particleInfo.Item4);
                return;
            }
            SpawnTapEffect(particlePosition).Forget();
        }

        private async UniTask SpawnJudgeEffect(float parentX, float childX, JudgementType judgementType)
        {
            if (judgeParticlePool == null) return;

            var judgeParticle = judgeParticlePool.Get();
            judgeParticle.Emit(
                parentX,
                childX,
                judgementType,
                addressableAssetManager.GetSprite(string.Format("judgetext_{0}", (int)judgementType)));
            await UniTask.WaitWhile(() => judgeParticle.IsPlaying, cancellationToken: destroyCancellationToken);
            judgeParticlePool.Release(judgeParticle);
        }

        private async UniTask SpawnTapEffect(float position)
        {
            if (tapParticlePool == null) return;

            var tapParticle = tapParticlePool.Get();
            tapParticle.SetMainTexture(addressableAssetManager.GetSprite("tap_effect"));
            tapParticle.Emit(position);
            await UniTask.WaitWhile(() => tapParticle.IsPlaying, cancellationToken: destroyCancellationToken);
            tapParticlePool.Release(tapParticle);
        }

        private async UniTask PlayHoldEffect((FingerType fingerType, NoteType noteType, JudgementType judgeType, int lane, IReadOnlyList<int> tappingLanes) particleInfo, float parentX, float childX)
        {
            if (!playingHoldParticleDict.ContainsKey(particleInfo.lane)
                && particleInfo.fingerType == FingerType.Down
                && particleInfo.judgeType != JudgementType.Miss)
            {
                if (holdParticlePool == null) return;

                var holdParticle = holdParticlePool.Get();
                holdParticle.SetMainTexture(addressableAssetManager.GetSprite("hold_effect"));
                holdParticle.Play(parentX);
                playingHoldParticleDict.Add(particleInfo.lane, holdParticle);
            }
            if (playingHoldParticleDict.TryGetValue(particleInfo.lane, out var playing)
                && !particleInfo.tappingLanes.Any(x => x == particleInfo.lane))
            {
                if (holdParticlePool == null) return;

                holdParticlePool.Release(playing);
                playingHoldParticleDict.Remove(particleInfo.lane);
                await SpawnJudgeEffect(parentX, childX, particleInfo.judgeType);
            }
        }

        private void StopHoldEffect(int lane)
        {
            if (playingHoldParticleDict.TryGetValue(lane, out var holdParticle))
            {
                holdParticle.Stop();
                holdParticlePool?.Release(holdParticle);
                playingHoldParticleDict.Remove(lane);
            }
        }

        private bool EnsurePools()
        {
            if (tapParticlePool != null && holdParticlePool != null && judgeParticlePool != null) return true;

            var tapPrefab = addressablePrefabResolver.GetComponentOrFallback("TapEffect", tapParticleObject);
            var holdPrefab = addressablePrefabResolver.GetComponentOrFallback("HoldEffect", holdParticleObject);
            var judgePrefab = addressablePrefabResolver.GetComponentOrFallback("TextJudge", judgeParticleObject);
            if (tapPrefab == null || holdPrefab == null || judgePrefab == null) return false;

            tapParticlePool ??= CreatePool(tapPrefab);
            holdParticlePool ??= CreatePool(holdPrefab);
            judgeParticlePool ??= CreatePool(judgePrefab);
            return true;
        }

        private void OnDestroy()
        {
            tapParticlePool?.Dispose();
            holdParticlePool?.Dispose();
            judgeParticlePool?.Dispose();
        }
    }
}