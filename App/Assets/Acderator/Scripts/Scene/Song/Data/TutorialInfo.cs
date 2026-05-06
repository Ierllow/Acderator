using Intense.Master;
using System.Collections.Generic;

namespace Song
{
    public class TutorialInfo
    {
        public TutorialMaster MTutorial { get; }
        public List<TutorialStepMaster> MTutorialStepList { get; }

        public TutorialInfo(TutorialMaster mTutorial, List<TutorialStepMaster> mTutorialStepList)
        {
            MTutorial = mTutorial;
            MTutorialStepList = mTutorialStepList;
        }
    }
}