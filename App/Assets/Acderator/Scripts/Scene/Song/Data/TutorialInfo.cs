using Intense.Master;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

namespace Song
{
    [System.Serializable]
    public class TutorialInfo
    {
        public TutorialMaster MTutorial { get; }
        public List<TutorialStepMaster> MTutorialStepList { get; }

        public TutorialInfo(int tid)
        {
            MTutorial = MasterDataManager.Instance.MemoryDatabase.TutorialMasterTable.FindByTid(tid);
            MTutorialStepList = MasterDataManager.Instance.MemoryDatabase.TutorialStepMasterTable.All.AsValueEnumerable().ToList();
        }
    }
}