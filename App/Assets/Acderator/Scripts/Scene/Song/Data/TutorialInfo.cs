using Intense.Master;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

namespace Song
{
    public class TutorialInfo
    {
        public TutorialMaster MTutorial { get; }
        public List<TutorialStepMaster> MTutorialStepList { get; }

        public TutorialInfo(int tid, MasterDataManager masterDataManager)
        {
            MTutorial = masterDataManager.MemoryDatabase.TutorialMasterTable.FindByTid(tid);
            MTutorialStepList = masterDataManager.MemoryDatabase.TutorialStepMasterTable.All.AsValueEnumerable().ToList();
        }
    }
}
