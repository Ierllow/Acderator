using System;
using CriWare;

namespace Intense
{
    public static class SoundExtensions
    {
        public static bool IsPlayEnd(this CriAtomExPlayer criAtomExPlayer) => criAtomExPlayer.GetStatus().EnumEquals(CriAtomExPlayer.Status.PlayEnd);
        public static bool IsPlaying(this CriAtomExPlayer criAtomExPlayer) => criAtomExPlayer.GetStatus().EnumEquals(CriAtomExPlayer.Status.Playing);
        public static bool IsStop(this CriAtomExPlayer criAtomExPlayer) => criAtomExPlayer.GetStatus().EnumEquals(CriAtomExPlayer.Status.Stop);
        public static float ToSeconds(this long time) => time / 1000f;
    }
}