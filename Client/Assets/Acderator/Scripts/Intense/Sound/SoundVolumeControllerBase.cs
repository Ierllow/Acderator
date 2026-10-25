using System.Collections.Generic;

namespace Intense
{
    public abstract class SoundVolumeControllerBase
    {
        private readonly HashSet<CriAtomSoundPlayer> players = new();

        protected abstract float CurrentVolume { get; }
        protected abstract bool IsMuted { get; }

        public void Register(CriAtomSoundPlayer player)
        {
            players.Add(player);
            player.UpdateVolume(CurrentVolume, IsMuted);
        }

        public void Unregister(CriAtomSoundPlayer player) => players.Remove(player);

        public void UpdateVolume(float volume, bool isMute = false)
        {
            foreach (var player in players)
            {
                player.UpdateVolume(volume, isMute);
            }
        }
    }
}