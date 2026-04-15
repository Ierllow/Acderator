
using Intense.Master;
using System.Collections.Generic;
using System.Linq;

namespace SongSelect
{
    public sealed class SongSelectSceneContext : SceneContext
    {
        public List<string> SongSelectBundleNameList(List<int> groupList)
            => new List<string>() { "song/jacket", "song/rank", "songselect/bg" }.Concat(groupList.Select(x => string.Format("sounds/song/song_{0}", x)).ToList()).ToList();

        public Song.SongSceneContext ToSongSceneContext(SongMaster mSong, bool isAuto, string sessionId) => Song.SongSceneContext.Create(new(mSong), isAuto, Song.ESongMode.Normal, sessionId);
    }
}