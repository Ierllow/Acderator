#nullable enable

using Intense.Master;
using System.Collections.Generic;
using System.Linq;

namespace Song
{
    public class TutorialData
    {
        public TutorialMaster TutorialMaster { get; init; } = default!;
        public List<TutorialStepMaster> StepList { get; init; } = new();
        public int CurrentStepIndex { get; set; }
        public float StartTime { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount => StepList.Count;
        public bool IsCompleted => CurrentStepIndex >= TotalCount;

        public TutorialStepMaster? GetCurrentStep() => StepList.ElementAtOrDefault(CurrentStepIndex);

        public void CompleteCurrentStep()
        {
            if (CurrentStepIndex >= TotalCount) return;

            CompletedCount++;
            CurrentStepIndex++;
        }
    }
}
