using Intense;

namespace Song
{
    public sealed class SongApplicationPauseHandler
    {
        private readonly SongControllerResolver songControllerResolver;
        private readonly NotesManager notesManager;

        private bool CanPause => !SoundManager.Instance.SongExPlayer.IsPlayEnd() && notesManager.AliveNoteList.Count != 0;

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