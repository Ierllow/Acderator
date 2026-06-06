using Intense;
using Intense.Asset;
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

        public void Setup(SongMaster mSong, ResultInfo resultInfo, Sprite jacketSprite, Sprite rankSprite)
        {
            jacket.SetSprite(jacketSprite);
            songName.SetText(mSong.Name);
            composer.SetText(mSong.Composer);
            perfectNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Perfect));
            greatNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Great));
            goodNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Good));
            badNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Bad));
            missNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(EJudgementType.Miss));
            score.SetText("{0:D7}", resultInfo.CurrentScore);
            highScore.SetText(string.Format("<color={0}>High Score</color> {1:D7}", "#C58EF1", (int)resultInfo.HighScore));
            rank.SetSprite(rankSprite);
        }
    }
}