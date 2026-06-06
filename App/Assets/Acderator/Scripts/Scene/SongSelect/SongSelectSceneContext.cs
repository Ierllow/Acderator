using Intense.Asset;
using Intense.Master;
using System.Collections.Generic;
using System.Linq;

namespace SongSelect
{
    public sealed class SongSelectSceneContext : SceneContext
    {
        public IReadOnlyList<AddressableAssetAddress> DynamicAssetAddressList(IEnumerable<int> groupList)
            => groupList.Select(DynamicAssetAddresses.Song).ToList();

        public Song.SongSceneContext ToSongSceneContext(SongMaster mSong, bool isAuto, string sessionId) => Song.SongSceneContext.Create(new(mSong), isAuto, Song.ESongMode.Normal, sessionId);
    }
}