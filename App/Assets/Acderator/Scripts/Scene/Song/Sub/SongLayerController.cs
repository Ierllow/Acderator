using Intense;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Song
{
    public class SongLayerController : MonoBehaviour
    {
        [SerializeField] private ComboView comboView;
        [SerializeField] private SongPlayingFieldView songPlayingFieldView;
        [SerializeField] private HpBar hpBar;
#if UNITY_EDITOR
        [SerializeField] private DebugInfoView debugInfoView;
        public DebugInfoView DebugInfoView => debugInfoView;
#endif
        [Inject] private SongGameLogic songGameLogic;
        [Inject] private SongResultCalculator songResultCalculator;

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

        public void InitScore() => songGameLogic.InitScore();

        public void UpdateSongLayer(FingerInfo fingerInfo) => songGameLogic.UpdateGameLogic(fingerInfo);

        public ESongResultType GetSongResult(int noteCount) => songResultCalculator.CalculateResult(songGameLogic.CurrentHpPercent, songGameLogic.JudgeCountDict, songGameLogic.CurrentCombo, noteCount);

        public void SetPauseButtonGrayOut(bool value) => songPlayingFieldView.PauseButton.SetGrayOut(value);

        public void Subscribes(Func<float, bool> predicate, CancellationToken cancellationToken = default)
        {
            songGameLogic.SubscribeHpUpdate(predicate, hpBar.SetHp).RegisterTo(cancellationToken);
            songGameLogic.SubscribeComboUpdate(predicate, comboView.UpdateComboNum).RegisterTo(cancellationToken);
            songGameLogic.SubscribeScoreUpdate(songPlayingFieldView.UpdateDisplay).RegisterTo(cancellationToken);
        }
    }
}