using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZLinq;
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

        [Inject] private MasterDataManager masterDataManager;

        public IUniTaskAsyncEnumerable<Toggle> EveryToggleChanged => UniTaskAsyncEnumerable.EveryValueChanged(toggleGroup, x => x.ActiveToggles().AsValueEnumerable().FirstOrDefault());

        private List<SongMaster> mSongList;

        public void SetData(int group, int selectedDifficulty)
        {
            mSongList = masterDataManager.MemoryDatabase.SongMasterTable.Where(x => x.Group == group).ToList();
            atlas.SetAtlasFormat("{0}", group, "song/jacket");
            foreach (var (toggle, index) in toggles.AsValueEnumerable().Select((x, i) => (x, i)))
            {
                toggle.SetToggleText(mSongList[index].Difficulty.ToString());
                toggle.Toggle.isOn = toggle.name == selectedDifficulty.ToString();
            }
        }

        public void UpdateInfo(int selectedDifficulty, int score, int percentNum)
        {
            if (mSongList == default) return;

            var sid = mSongList.AsValueEnumerable().First(x => x.Difficulty == selectedDifficulty).Sid;
            hightScore.SetTextFormat("{0:D7}", score);
            percent.SetTextFormat("{0:F1}{1}", percentNum, "%");
            rank.SetAtlasFormat("icon_result_rank_{0}", (int)ScoreUtils.ToRank(score, true), "song/rank");
        }
    }
}