using Cysharp.Threading.Tasks;
using Intense;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZLinq;

namespace Song
{
    public class SongParticleController : MonoBehaviour, IController
    {
        [SerializeField] private ParticleObject tapParticleObject;
        [SerializeField] private HoldParticleObject holdParticleObject;
        [SerializeField] private JudgeParticleObject judgeParticleObject;
        [SerializeField] private Transform particlePoolTransform;

        private TapParticlePool tapParticlePool;
        private HoldParticlePool holdParticlePool;
        private JudgeParticlePool judgeParticlePool;

        private readonly Dictionary<int, HoldParticleObject> playingHoldParticleDict = new();

        private void Awake()
        {
            tapParticlePool = new(
                createFunc: () => Instantiate(tapParticleObject, particlePoolTransform),
                actionOnGet: note => note.gameObject.SetActive(true),
                actionOnRelease: note => note.gameObject.SetActive(false),
                actionOnDestroy: note => Destroy(note.gameObject)
            );
            holdParticlePool = new(
                createFunc: () => Instantiate(holdParticleObject, particlePoolTransform),
                actionOnGet: note => note.gameObject.SetActive(true),
                actionOnRelease: note => note.gameObject.SetActive(false),
                actionOnDestroy: note => Destroy(note.gameObject)
            );
            judgeParticlePool = new(
                createFunc: () => Instantiate(judgeParticleObject, particlePoolTransform),
                actionOnGet: note => note.gameObject.SetActive(true),
                actionOnRelease: note => note.gameObject.SetActive(false),
                actionOnDestroy: note => Destroy(note.gameObject)
            );
        }

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
            if (particleInfo.Item2.EnumEquals(ENoteType.Long) || particleInfo.Item2.EnumEquals(ENoteType.Curve))
            {
                if (!particleInfo.Item1.EnumEquals(EFingerType.Up))
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

        private async UniTask PlayHoldEffect((EFingerType fingerType, ENoteType noteType, EJudgementType judgeType, int lane, List<NoteBase> tappingNotes) particleInfo, float parentX, float childX)
        {
            if (!playingHoldParticleDict.ContainsKey(particleInfo.lane)
                && particleInfo.fingerType.EnumEquals(EFingerType.Down)
                && !particleInfo.judgeType.EnumEquals(EJudgementType.Miss))
            {
                var holdParticle = holdParticlePool.Get();
                holdParticle.Play(parentX);
                playingHoldParticleDict.Add(particleInfo.lane, holdParticle);
            }
            if (playingHoldParticleDict.TryGetValue(particleInfo.lane, out var playing)
                && !particleInfo.tappingNotes.AsValueEnumerable().Any(x => x.NoteData.Lane == particleInfo.lane))
            {
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