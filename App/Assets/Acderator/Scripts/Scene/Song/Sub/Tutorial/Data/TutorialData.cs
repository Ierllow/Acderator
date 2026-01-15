using Intense.Master;
using System.Collections.Generic;
using ZLinq;

namespace Song
{
    public class TutorialData
    {
        public TutorialMaster TutorialMaster { get; init; }
        public List<TutorialStepMaster> StepList { get; init; } = new();
        public int CurrentStepIndex { get; set; }
        public float StartTime { get; set; }
        public bool IsCompleted { get; set; }

        public int CompletedCount { get; set; }
        public int TotalCount => StepList.AsValueEnumerable().Count();

        public TutorialStepMaster GetCurrentStep() => StepList.AsValueEnumerable().ElementAtOrDefault(CurrentStepIndex);

        public void CompleteCurrentStep()
        {
            if (CurrentStepIndex >= TotalCount) return;

            CompletedCount++;
            CurrentStepIndex++;
            IsCompleted = CurrentStepIndex >= TotalCount;
        }

        public void SkipCurrentStep()
        {
            if (CurrentStepIndex >= TotalCount) return;

            CurrentStepIndex++;
            IsCompleted = CurrentStepIndex >= TotalCount;
        }
    }
}
