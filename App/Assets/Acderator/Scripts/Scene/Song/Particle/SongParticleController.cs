using Cysharp.Threading.Tasks;
using Intense;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ZLinq;

namespace Song
{
    public class SongParticleController : IController
    {
        [Inject] private TapParticlePool tapPool;
        [Inject] private HoldParticlePool holdPool;
        [Inject] private JudgeParticlePool judgePool;

        private readonly Dictionary<int, HoldParticleObject> playingHoldParticleDict = new();

        public void UpdateParticles(FingerInfo fingerInfo)
        {
            if (fingerInfo.IsMissed) StopHoldEffect(fingerInfo.NoteBase.NoteData.Lane);
            else UpdateParticles((fingerInfo.FingerType, fingerInfo.NoteBase.NoteData.NoteType, fingerInfo.JudgmentType, fingerInfo.NoteBase.NoteData.Lane, fingerInfo.TappingNoteList));
        }

        private void UpdateParticles((EFingerType, ENoteType, EJudgementType, int, List<NoteBase>) particleInfo)
        {
            if (particleInfo.Item3.EnumEquals(EJudgementType.None)) return;

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
            if (particleInfo.Item2.EnumEquals(ENoteType.Long)
                || particleInfo.Item2.EnumEquals(ENoteType.Curve))
            {
                PlayHoldEffect(particleInfo, particlePosition, judgeParticlePosition).Forget();
                return;
            }
            SpawnTapEffect(particlePosition).Forget();
        }

        private async UniTask SpawnJudgeEffect(float parentX, float childX, EJudgementType judgementType)
        {
            var pool = judgePool.Spawn(parentX, childX, judgementType);
            await UniTask.WaitWhile(() => pool.IsPlaying);
            judgePool.Despawn(pool);
        }

        private async UniTask SpawnTapEffect(float x)
        {
            var pool = tapPool.Spawn(x);
            await UniTask.WaitWhile(() => pool.IsPlaying);
            tapPool.Despawn(pool);
        }

        private async UniTask PlayHoldEffect((EFingerType fingerType, ENoteType noteType, EJudgementType judgeType, int lane, List<NoteBase> tappingNotes) particleInfo, float parentX, float childX)
        {
            if (!playingHoldParticleDict.ContainsKey(particleInfo.lane)
                && particleInfo.fingerType.EnumEquals(EFingerType.Down)
                && !particleInfo.judgeType.EnumEquals(EJudgementType.Miss))
            {
                var holdParticle = holdPool.Spawn(parentX);
                playingHoldParticleDict.Add(particleInfo.lane, holdParticle);
            }
            if (playingHoldParticleDict.TryGetValue(particleInfo.lane, out var playing)
                && !particleInfo.tappingNotes.AsValueEnumerable().Any(x => x.NoteData.Lane == particleInfo.lane))
            {
                holdPool.Despawn(playing);
                playingHoldParticleDict.Remove(particleInfo.lane);
                await SpawnJudgeEffect(parentX, childX, particleInfo.judgeType);
            }
        }

        private void StopHoldEffect(int lane)
        {
            if (playingHoldParticleDict.TryGetValue(lane, out var holdParticle))
            {
                holdParticle.Stop();
                holdPool.Despawn(holdParticle);
                playingHoldParticleDict.Remove(lane);
            }
        }
    }
}