using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Master;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Song
{
    public class ScoreController
    {
        [Inject] private MasterDataManager masterDataManager;

        public float CurrentScore { get; private set; } = 0f;

        public IUniTaskAsyncEnumerable<float> EveryUpdateScoreAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.CurrentScore).Queue();

        private readonly Queue<int> scoreQueue = new();
        private readonly Dictionary<EJudgementType, float> rateCacheDict = new();

        private int noteCount = 0;
        private int maxScore = 0;

        public void Init(int sid, int noteCount)
        {
            this.noteCount = noteCount;
            maxScore = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sid).Score;

            foreach (var row in masterDataManager.MemoryDatabase.SongScoreRateMasterTable.All)
            {
                rateCacheDict[(EJudgementType)row.Type] = row.Rate;
            }

            var perNoteScore = maxScore / noteCount;
            var remainder = maxScore % noteCount;

            for (var i = 0; i < noteCount; i++)
            {
                scoreQueue.Enqueue(perNoteScore + (i < remainder ? 1 : 0));
            }
        }

        public void AddScore(EJudgementType judgmentType)
        {
            switch (judgmentType)
            {
                case EJudgementType.None:
                case EJudgementType.Miss:
                case EJudgementType.Bad:
                    return;
            }

            if (!scoreQueue.TryDequeue(out var baseScore)) return;
            if (!rateCacheDict.TryGetValue(judgmentType, out var rate)) return;

            CurrentScore += Mathf.RoundToInt(baseScore * rate);
            if (CurrentScore == maxScore) CurrentScore += noteCount;
        }
    }
}