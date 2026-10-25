#nullable enable

using Intense.Master;
using System.Collections.Generic;

namespace Song
{
    public class TutorialInfo
    {
        public TutorialMaster Tutorial { get; }
        public List<TutorialStepMaster> TutorialSteps { get; }

        public TutorialInfo(TutorialMaster tutorial, List<TutorialStepMaster> tutorialSteps)
        {
            Tutorial = tutorial;
            TutorialSteps = tutorialSteps;
        }
    }
}