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

        public IUniTaskAsyncEnumerable<Toggle> EveryToggleChanged => UniTaskAsyncEnumerable.EveryValueChanged(toggleGroup, x => x.ActiveToggles().AsValueEnumerable().FirstOrDefault());

        private List<SongMaster> mSongList;

        public void SetData(int group, int selectedDifficulty)
        {
            mSongList = MasterDataManager.Instance.MemoryDatabase.SongMasterTable.Where(x => x.Group == group).ToList();
            atlas.SetAtlasFormat("{0}", group, "song/jacket");
            foreach (var (toggle, index) in toggles.AsValueEnumerable().Select((x, i) => (x, i)))
            {
                toggle.SetToggleText(mSongList.AsValueEnumerable().ElementAt(index).Difficulty.ToString());
                toggle.Toggle.isOn = toggle.name == selectedDifficulty.ToString();
            }
            UpdateInfo(selectedDifficulty);
        }

        public void UpdateInfo(int selectedDifficulty)
        {
            if (mSongList == default) return;

            var sid = mSongList.AsValueEnumerable().First(x => x.Difficulty == selectedDifficulty).Sid;

            var getScore = ScoreManager.Instance.GetScore(sid);
            hightScore.SetTextFormat("{0:D7}", getScore);
            percent.SetTextFormat("{0:F1}{1}", ScoreUtils.ToScorePercent(sid), "%");
            rank.SetAtlasFormat("icon_result_rank_{0}", (int)ScoreUtils.ToRank(getScore, true), "song/rank");
        }
    }
}