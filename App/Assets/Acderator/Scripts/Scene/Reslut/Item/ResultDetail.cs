using Cysharp.Text;
using Intense;
using Intense.Data;
using Intense.Master;
using Intense.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Result
{
    public class ResultDetail : MonoBehaviour
    {
        [SerializeField] private AtlasImage jacket;
        [SerializeField] private AtlasImage rank;
        [SerializeField] private TextMeshProUGUI songName;
        [SerializeField] private TextMeshProUGUI composer;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private TextMeshProUGUI highScore;
        [SerializeField] private TextMeshProUGUI perfectNum;
        [SerializeField] private TextMeshProUGUI greatNum;
        [SerializeField] private TextMeshProUGUI goodNum;
        [SerializeField] private TextMeshProUGUI badNum;
        [SerializeField] private TextMeshProUGUI missNum;

        public void Setup(ResultInfo resultInfo)
        {
            var mSong = MasterDataManager.Instance.MemoryDatabase.SongMasterTable.FindBySid(resultInfo.Sid);

            jacket.SetAtlasFormat("{0}", mSong.Group, "song/jacket");
            songName.SetTextFormat("{0}", mSong.Name);
            composer.SetTextFormat("{0}", mSong.Composer);
            perfectNum.SetTextFormat("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Perfect));
            greatNum.SetTextFormat("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Great));
            goodNum.SetTextFormat("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Good));
            badNum.SetTextFormat("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Bad));
            missNum.SetTextFormat("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Miss));
            score.SetTextFormat("{0:D7}", resultInfo.CurrentScore);
            highScore.SetTextFormat("<color={0}>High Score</color> {1:D7}", "#C58EF1", (int)resultInfo.HighScore);
            rank.SetAtlasFormat("icon_result_rank_{0}", (int)ScoreUtils.ToRank(resultInfo.CurrentScore), "song/rank");
        }
    }
}