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

        public void Setup(SongMaster song, ResultInfo resultInfo, Sprite jacketSprite, Sprite rankSprite)
        {
            jacket.SetSprite(jacketSprite);
            songName.SetText(song.Name);
            composer.SetText(song.Composer);
            perfectNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(JudgementType.Perfect));
            greatNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(JudgementType.Great));
            goodNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(JudgementType.Good));
            badNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(JudgementType.Bad));
            missNum.SetText("{0:D3}", resultInfo.JudgeCountDict.GetValueOrDefault(JudgementType.Miss));
            score.SetText("{0:D7}", resultInfo.CurrentScore);
            highScore.SetText(string.Format("<color={0}>High Score</color> {1:D7}", "#C58EF1", (int)resultInfo.HighScore));
            rank.SetSprite(rankSprite);
        }
    }
}