using Intense.Master;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Song
{
    internal class TutorialSceneContextBuilder
    {
        [Inject] private MasterDataManager masterDataManager;

        public bool TryBuild(SongMaster selectedSong, string sessionId, out SongSceneContext tutorialSceneContext)
        {
            var tutorial = FindTutorial(selectedSong);
            var steps = GetTutorialSteps();
            if (tutorial == default || steps.Count == 0)
            {
                tutorialSceneContext = default;
                return false;
            }

            tutorialSceneContext = CreateTutorialSceneContext(tutorial, steps, selectedSong, sessionId);
            return true;
        }

        public bool TryGetFirstTutorialScoreId(out int scoreId)
        {
            var tutorialSong = FindFirstTutorialSong();
            scoreId = tutorialSong?.Sid ?? default;
            return tutorialSong != default;
        }

        public bool TryBuildFirstTutorial(string sessionId, out SongSceneContext tutorialSceneContext)
        {
            var tutorialSong = FindFirstTutorialSong();
            var tutorial = tutorialSong == default ? default : FindTutorial(tutorialSong);
            var steps = GetTutorialSteps();
            if (tutorialSong == default || tutorial == default || steps.Count == 0)
            {
                tutorialSceneContext = default;
                return false;
            }

            tutorialSceneContext = CreateTutorialSceneContext(tutorial, steps, tutorialSong, sessionId);
            return true;
        }

        private TutorialMaster FindTutorial(SongMaster selectedSong) => masterDataManager.MemoryDatabase.TutorialMasterTable.Where(x => x.Sid == selectedSong.Sid || x.Sid == selectedSong.Group || x.Sid == 0).OrderBy(x => x.Order).FirstOrDefault();

        private List<TutorialStepMaster> GetTutorialSteps() => masterDataManager.MemoryDatabase.TutorialStepMasterTable.OrderBy(x => x.StepOrder).ToList();

        private SongMaster FindFirstTutorialSong()
        {
            var tutorial = masterDataManager.MemoryDatabase.TutorialMasterTable.OrderBy(x => x.Order).FirstOrDefault();
            return tutorial == default ? default : masterDataManager.MemoryDatabase.SongMasterTable.FindBySid(tutorial.Sid);
        }

        private static SongSceneContext CreateTutorialSceneContext(TutorialMaster tutorial, List<TutorialStepMaster> steps, SongMaster song, string sessionId) => SongSceneContext.Create(new SongInfo(song), sessionId, new TutorialInfo(tutorial, steps.OrderBy(x => x.StepOrder).ToList()));
    }
}