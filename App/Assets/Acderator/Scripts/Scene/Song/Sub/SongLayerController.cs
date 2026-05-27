using Intense;
using Intense.Data;
using R3;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Song
{
    public class SongLayerController : MonoBehaviour
    {
        [SerializeField] private ComboView comboView;
        [SerializeField] private SongPlayingFieldView songPlayingFieldView;
        [SerializeField] private HpBar hpBar;

        [Inject] private readonly SongGameLogic songGameLogic;

        private const int MinClearHpPercent = 70;

        public Observable<Unit> OnTapPauseButtonAsObservable => songPlayingFieldView.PauseButton.OnTapButtonAsObservable;

        public int CurrentScore => songGameLogic.CurrentScore;
        public Dictionary<EJudgementType, int> JudgeCountDict => songGameLogic.JudgeCountDict;

        public void Show(bool isAuto)
        {
            songPlayingFieldView.Setup();
            hpBar.gameObject.SetActive(!isAuto);
            songPlayingFieldView.SetAutoTextActive(isAuto);
            hpBar.Move();
        }

        public void Init(int sid) => songGameLogic.Init(sid);

        public void UpdateSongLayer(FingerInfo fingerInfo) => songGameLogic.UpdateGameLogic(fingerInfo);

        public ESongResultType GetSongResult(int noteCount) => true switch
        {
            _ when (int)(songGameLogic.CurrentHpPercent * HpBarController.MAX_HP_PERCENT) < MinClearHpPercent => ESongResultType.Failed,
            _ when noteCount == songGameLogic.JudgeCountDict.GetValueOrDefault(EJudgementType.Perfect) => ESongResultType.Excellent,
            _ when noteCount == songGameLogic.CurrentCombo => ESongResultType.FullCombo,
            _ when ScoreUtils.IsClear(songGameLogic.CurrentCombo) => ESongResultType.Clear,
            _ => ESongResultType.Failed,
        };

        public void SetPauseButtonGrayOut(bool value) => songPlayingFieldView.PauseButton.SetGrayOut(value);
    }
}