#nullable enable

using Intense;
using Intense.Master;
using R3;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Song
{
    public class ScoreController
    {
        [Inject] private readonly MasterDataManager masterDataManager = default!;

        private readonly ReactiveProperty<float> currentScore = new(0f);
        public ReadOnlyReactiveProperty<float> CurrentScore => currentScore;

        private readonly Queue<int> scoreQueue = new();
        private readonly Dictionary<JudgementType, float> rateCacheDict = new();

        private int noteCount = 0;
        private int maxScore = 0;

        public void Init(int sid, int noteCount)
        {
            this.noteCount = noteCount;
            maxScore = masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(sid).Score;

            foreach (var row in masterDataManager.MemoryDatabase.SongScoreRateMasterTable.All)
            {
                rateCacheDict[(JudgementType)row.Type] = row.Rate;
            }

            var perNoteScore = maxScore / noteCount;
            var remainder = maxScore % noteCount;

            for (var i = 0; i < noteCount; i++)
            {
                scoreQueue.Enqueue(perNoteScore + (i < remainder ? 1 : 0));
            }
        }

        public void AddScore(JudgementType judgmentType)
        {
            switch (judgmentType)
            {
                case JudgementType.None:
                case JudgementType.Miss:
                case JudgementType.Bad:
                    return;
            }

            if (!scoreQueue.TryDequeue(out var baseScore)) return;
            if (!rateCacheDict.TryGetValue(judgmentType, out var rate)) return;

            currentScore.Value += Mathf.RoundToInt(baseScore * rate);
            if (currentScore.Value == maxScore) currentScore.Value += noteCount;
        }
    }
}