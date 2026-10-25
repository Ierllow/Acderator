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

        public Song.SongSceneContext ToSongSceneContext(SongMaster song, bool isAuto, string sessionId) => Song.SongSceneContext.Create(new(song), isAuto, Song.SongMode.Normal, sessionId);
    }
}