#nullable enable

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
        [SerializeField] private SongPlayingFieldView songPlayingFieldView = default!;
        [SerializeField] private HpBar hpBar = default!;

        [Inject] private readonly SongGameLogic songGameLogic = default!;

        private const int MinClearHpPercent = 70;

        public Observable<Unit> OnTapPauseButtonAsObservable => songPlayingFieldView.PauseButton.OnTapButtonAsObservable;

        public int CurrentScore => songGameLogic.CurrentScore;
        public Dictionary<JudgementType, int> JudgeCountDict => songGameLogic.JudgeCountDict;

        public void Show(bool isAuto)
        {
            songPlayingFieldView.Setup();
            hpBar.gameObject.SetActive(!isAuto);
            songPlayingFieldView.SetAutoTextActive(isAuto);
            hpBar.Move();
        }

        public void Init(int sid) => songGameLogic.Init(sid);

        public void UpdateSongLayer(FingerInfo fingerInfo) => songGameLogic.UpdateGameLogic(fingerInfo);

        public SongResultType GetSongResult(int noteCount) => true switch
        {
            _ when (int)(songGameLogic.CurrentHpPercent * HpBarController.MaxHpPercent) < MinClearHpPercent => SongResultType.Failed,
            _ when noteCount == songGameLogic.JudgeCountDict.GetValueOrDefault(JudgementType.Perfect) => SongResultType.Excellent,
            _ when noteCount == songGameLogic.CurrentCombo => SongResultType.FullCombo,
            _ => SongResultType.Clear,
        };

        public void SetPauseButtonGrayOut(bool value) => songPlayingFieldView.PauseButton.SetGrayOut(value);
    }
}