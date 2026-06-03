using CriWare;

namespace Intense
{
    public sealed class CriAtomSoundPlayer
    {
        private readonly CriAtomExPlayer exPlayer;

        public bool IsFading => exPlayer.IsFading();
        public long Time => exPlayer.GetTime();

        public CriAtomSoundPlayer(bool enableAudioSyncedTimer = false) => exPlayer = new CriAtomExPlayer(enableAudioSyncedTimer);

        public void UpdateVolume(float volume, bool isMute = false)
        {
            exPlayer.SetVolume(!isMute ? volume : 0f);
            exPlayer.UpdateAll();
        }

        public CriAtomPlaybackSession Play(CriAtomCueSheet sheet, string cueName, bool isLoop = false)
        {
            Stop();
            exPlayer.SetCue(sheet.acb, cueName);
            exPlayer.Loop(isLoop);
            return new CriAtomPlaybackSession(exPlayer, exPlayer.Start());
        }

        public CriAtomPlaybackSession Play(CriAtomCueSheet sheet, string cueName, int startTime, bool isLoop = false)
        {
            Stop();
            exPlayer.SetCue(sheet.acb, cueName);
            exPlayer.SetStartTime(startTime);
            exPlayer.Loop(isLoop);
            return new CriAtomPlaybackSession(exPlayer, exPlayer.Start());
        }

        public CriAtomPlaybackSession Start() => new(exPlayer, exPlayer.Start());

        public void Pause(bool isPause) => exPlayer.Pause(isPause);

        public void Stop() => exPlayer.Stop(true);

        public void AttachFader() => exPlayer.AttachFader();

        public void SetFadeInTime(int time) => exPlayer.SetFadeInTime(time);

        public void SetFadeOutTime(int time) => exPlayer.SetFadeOutTime(time);

        public void SetFadeInStartOffset(int time) => exPlayer.SetFadeInStartOffset(time);
    }
}