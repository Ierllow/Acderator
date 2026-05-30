using Intense;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Song
{
    public class SongGameLogic
    {
        [Inject] private readonly ScoreController scoreController;
        [Inject] private readonly ComboController comboController;
        [Inject] private readonly HpBarController hpBarController;
        [Inject] private readonly NotesManager notesManager;

        private readonly Dictionary<EJudgementType, int> judgeCountDict = new()
        {
            { EJudgementType.None, 0 },
            { EJudgementType.Perfect, 0 },
            { EJudgementType.Great, 0 },
            { EJudgementType.Good, 0 },
            { EJudgementType.Bad, 0 },
            { EJudgementType.Miss, 0 },
        };

        public Dictionary<EJudgementType, int> JudgeCountDict => judgeCountDict;
        public int CurrentScore => (int)scoreController.CurrentScore.CurrentValue;
        public int CurrentCombo => comboController.CurrentCombo.CurrentValue;
        public float CurrentHpPercent => hpBarController.CurrentHpPercent.CurrentValue;

        public void Init(int sid)
        {
            scoreController.Init(sid, notesManager.LoadedChartInfo.NoteCount);
            hpBarController.Init(sid);
        }

        public void UpdateGameLogic(FingerInfo fingerInfo)
        {
            if (fingerInfo.NoteBase == default) return;
            if (!notesManager.AliveNoteList.Any(x => x == fingerInfo.NoteBase)) return;

            var noteType = fingerInfo.NoteData.NoteType == ENoteType.Flick;
            if (fingerInfo.IsMissed)
            {
                notesManager.RemoveNote(fingerInfo.NoteBase);
                comboController.UpdateCombo(fingerInfo.JudgmentType);
                judgeCountDict[fingerInfo.JudgmentType] += (fingerInfo.MissInfo?.missLongNote ?? false) ? 2 : 1;
                scoreController.AddScore(fingerInfo.JudgmentType);
            }
            else
            {
                if (fingerInfo.NoteData.NoteType == ENoteType.Flick && fingerInfo.FingerType == EFingerType.Down) return;
                comboController.UpdateCombo(fingerInfo.JudgmentType);
                judgeCountDict[fingerInfo.JudgmentType] += 1;
                scoreController.AddScore(fingerInfo.JudgmentType);
                if (fingerInfo.NoteData.NoteType == ENoteType.Long && fingerInfo.FingerType == EFingerType.Down) return;
                notesManager.RemoveNote(fingerInfo.NoteBase);
            }
            if (notesManager.SongOption.IsAuto) hpBarController.UpdateHp(fingerInfo.JudgmentType, notesManager.LoadedChartInfo.NoteCount);
        }
    }
}