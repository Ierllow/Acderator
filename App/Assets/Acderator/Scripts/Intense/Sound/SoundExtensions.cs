using CriWare;

namespace Intense
{
    public static class SoundExtensions
    {
        public static bool IsPlayEnd(this CriAtomExPlayer criAtomExPlayer) => criAtomExPlayer.GetStatus() == CriAtomExPlayer.Status.PlayEnd;
        public static float ToSeconds(this long time) => time / 1000f;
    }
}