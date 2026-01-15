
using Cysharp.Text;
using Intense.Master;
using System.Collections.Generic;
using ZLinq;

namespace SongSelect
{
    public sealed class SongSelectSceneContext : SceneContext
    {
        public List<string> SongSelectBundleNameList
        {
            get
            {
                var bundleNameList = new List<string>()
                {
                    "song/jacket",
                    "song/rank",
                    "songselect/bg",
                };
                var songMasterTable = MasterDataManager.Instance.MemoryDatabase.SongMasterTable;
                bundleNameList.AddRange(songMasterTable.Select(x => ZString.Format("sounds/song/song_{0}", x.Group)).ToList());
                return bundleNameList;
            }
        }

        public Song.SongSceneContext ToSongSceneContext(int sid, bool isAuto) => Song.SongSceneContext.Create(new(sid), isAuto, Song.ESongMode.Normal);
    }
}