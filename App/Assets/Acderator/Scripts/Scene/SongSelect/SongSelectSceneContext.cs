
using Cysharp.Text;
using Intense.Master;
using System.Collections.Generic;
using ZLinq;

namespace SongSelect
{
    public sealed class SongSelectSceneContext : SceneContext
    {
        public List<string> SongSelectBundleNameList(List<int> groupList)
            => new List<string>() { "song/jacket", "song/rank", "songselect/bg" }.AsValueEnumerable().Concat(groupList.AsValueEnumerable().Select(x => ZString.Format("sounds/song/song_{0}", x)).ToList()).ToList();

        public Song.SongSceneContext ToSongSceneContext(SongMaster mSong, bool isAuto, string sessionId) => Song.SongSceneContext.Create(new(mSong), isAuto, Song.ESongMode.Normal, sessionId);
    }
}