using Intense;
using Zenject;

namespace Song
{
    public sealed class SongApplicationPauseHandler
    {

        [Inject] private SoundManager soundManager;
        private readonly SongControllerResolver songControllerResolver;
        private readonly NotesManager notesManager;

        private bool CanPause => !soundManager.SongExPlayer.IsPlayEnd() && notesManager.AliveNoteList.Count != 0;

        public SongApplicationPauseHandler(SongControllerResolver songControllerResolver, NotesManager notesManager)
        {
            this.songControllerResolver = songControllerResolver;
            this.notesManager = notesManager;
        }

        public bool IsHandlePause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (!CanPause) return false;
                songControllerResolver.Loop.UpdateState(ESongState.Stop);
                return false;
            }
            return false;
        }
    }
}
