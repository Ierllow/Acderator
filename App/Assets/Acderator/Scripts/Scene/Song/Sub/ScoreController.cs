using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense;
using Intense.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

namespace Song
{
    public class ScoreController
    {
        public float CurrentScore { get; private set; } = 0f;

        public IUniTaskAsyncEnumerable<float> EveryUpdateScoreAsAsyncEnumerable => UniTaskAsyncEnumerable.EveryValueChanged(this, x => x.CurrentScore).Queue();

        private readonly Queue<int> scoreQueue = new();
        private int noteCount = 0;

        public void Init(int noteCount)
        {
            this.noteCount = noteCount;
            var maxScore = MasterDataManager.Instance.MemoryDatabase.SongBaseScoreMasterTable.First().Score;
            var perNoteScore = maxScore / noteCount;
            var remainder = maxScore % noteCount;
            ValueEnumerable.Repeat(0, noteCount).Select(i => perNoteScore + (i < remainder ? 1 : 0)).ToList().ForEach(scoreQueue.Enqueue);
        }

        public void AddScore(EJudgementType judgmentType)
        {
            switch (judgmentType)
            {
                case EJudgementType.None or EJudgementType.Miss or EJudgementType.Bad:
                    break;
                default:
                    if (scoreQueue.TryDequeue(out var baseScore))
                    {
                        var rate = MasterDataManager.Instance.MemoryDatabase.SongScoreRateMasterTable.First(x => x.Type == judgmentType.GetLength()).Rate;
                        CurrentScore += Mathf.RoundToInt(baseScore * rate);
                        var maxScore = MasterDataManager.Instance.MemoryDatabase.SongBaseScoreMasterTable.First().Score;
                        if (CurrentScore == maxScore)
                        {
                        }
                    }
                    break;
            }
        }
    }
}