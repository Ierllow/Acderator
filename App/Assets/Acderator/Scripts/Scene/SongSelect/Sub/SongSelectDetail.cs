using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SongSelect
{
    public class SongSelectDetail : MonoBehaviour
    {
        [SerializeField] private AtlasImage atlas;
        [SerializeField] private AtlasImage rank;
        [SerializeField] private TextMeshProUGUI hightScore;
        [SerializeField] private TextMeshProUGUI percent;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private ToggleEx[] toggles;

        [Inject] private readonly MasterDataManager masterDataManager;

        public IUniTaskAsyncEnumerable<Toggle> EveryToggleChanged => UniTaskAsyncEnumerable.EveryValueChanged(toggleGroup, x => x.ActiveToggles().FirstOrDefault());

        private List<SongMaster> mSongList;

        public void SetData(int group, int selectedDifficulty)
        {
            mSongList = masterDataManager.MemoryDatabase.SongMasterTable.Where(x => x.Group == group).ToList();
            atlas.SetAtlasFormat("{0}", group, "song/jacket");
            foreach (var (toggle, index) in toggles.Select((x, i) => (x, i)))
            {
                toggle.SetToggleText(mSongList[index].Difficulty.ToString());
                toggle.Toggle.isOn = toggle.name == selectedDifficulty.ToString();
            }
        }

        public void UpdateInfo(int selectedDifficulty, int score, int percentNum)
        {
            if (mSongList == default) return;

            var sid = mSongList.First(x => x.Difficulty == selectedDifficulty).Sid;
            hightScore.SetText("{0:D7}", score);
            percent.SetText(string.Format("{0:F1}{1}", percentNum, "%"));
            rank.SetAtlasFormat("icon_result_rank_{0}", (int)ScoreUtils.ToRank(score, true), "song/rank");
        }
    }
}