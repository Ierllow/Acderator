
using Cysharp.Text;
using Intense.Master;
using System.Collections.Generic;
using ZLinq;

namespace SongSelect
{
    public sealed class SongSelectSceneContext : SceneContext
    {
        private readonly MasterDataManager masterDataManager;

        public SongSelectSceneContext(MasterDataManager masterDataManager)
        {
            this.masterDataManager = masterDataManager;
        }

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
                var songMasterTable = masterDataManager.MemoryDatabase.SongMasterTable;
                bundleNameList.AddRange(songMasterTable.Select(x => ZString.Format("sounds/song/song_{0}", x.Group)).ToList());
                return bundleNameList;
            }
        }

        public Song.SongSceneContext ToSongSceneContext(int sid, bool isAuto, string sessionId) => Song.SongSceneContext.Create(new(sid, masterDataManager), isAuto, Song.ESongMode.Normal, sessionId);
    }
}
