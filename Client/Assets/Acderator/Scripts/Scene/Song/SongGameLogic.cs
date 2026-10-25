#nullable enable

using Intense;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Song
{
    public class SongGameLogic
    {
        [Inject] private readonly ScoreController scoreController = default!;
        [Inject] private readonly ComboController comboController = default!;
        [Inject] private readonly HpBarController hpBarController = default!;
        [Inject] private readonly NotesManager notesManager = default!;

        private readonly Dictionary<JudgementType, int> judgeCountDict = new()
        {
            { JudgementType.None, 0 },
            { JudgementType.Perfect, 0 },
            { JudgementType.Great, 0 },
            { JudgementType.Good, 0 },
            { JudgementType.Bad, 0 },
            { JudgementType.Miss, 0 },
        };

        public Dictionary<JudgementType, int> JudgeCountDict => judgeCountDict;
        public int CurrentScore => (int)scoreController.CurrentScore.CurrentValue;
        public int CurrentCombo => comboController.CurrentCombo.CurrentValue;
        public float CurrentHpPercent => hpBarController.CurrentHpPercent.CurrentValue;

        public void Init(int sid)
        {
            scoreController.Init(sid, notesManager.LoadedChartInfo!.NoteCount);
            hpBarController.Init(sid);
        }

        public void UpdateGameLogic(FingerInfo fingerInfo)
        {
            if (fingerInfo.NoteBase == null || fingerInfo.NoteData == null) return;
            if (!notesManager.AliveNoteList.Any(x => x == fingerInfo.NoteBase)) return;

            if (fingerInfo.IsMissed)
            {
                notesManager.RemoveNote(fingerInfo.NoteBase);
                comboController.UpdateCombo(fingerInfo.JudgmentType);
                judgeCountDict[fingerInfo.JudgmentType] += (fingerInfo.MissInfo?.missLongNote ?? false) ? 2 : 1;
                scoreController.AddScore(fingerInfo.JudgmentType);
            }
            else
            {
                if (fingerInfo.NoteData.NoteType == NoteType.Flick && fingerInfo.FingerType == FingerType.Down) return;
                comboController.UpdateCombo(fingerInfo.JudgmentType);
                judgeCountDict[fingerInfo.JudgmentType] += 1;
                scoreController.AddScore(fingerInfo.JudgmentType);
                if (fingerInfo.NoteData.NoteType == NoteType.Long && fingerInfo.FingerType == FingerType.Down) return;
                notesManager.RemoveNote(fingerInfo.NoteBase);
            }
            if (!notesManager.SongOption.IsAuto) hpBarController.UpdateHp(fingerInfo.JudgmentType, notesManager.LoadedChartInfo!.NoteCount);
        }
    }
}