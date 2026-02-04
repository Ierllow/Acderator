using Cysharp.Threading.Tasks.Linq;
using Intense;
using System;
using System.Collections.Generic;
using Zenject;
using ZLinq;

namespace Song
{
    public class SongGameLogic
    {
        [Inject] private ScoreController scoreController;
        [Inject] private ComboController comboController;
        [Inject] private HpBarController hpBarController;
        [Inject] private NotesManager notesManager;

        private readonly JudgmentCounter judgmentCounter = new();

        public Dictionary<EJudgementType, int> JudgeCountDict => judgmentCounter.GetJudgmentCountDict();
        public int CurrentScore => (int)scoreController.CurrentScore;
        public int CurrentCombo => comboController.CurrentCombo;
        public float CurrentHpPercent => hpBarController.CurrentHpPercent;

        public void InitScore() => scoreController.Init(notesManager.LoadedChartInfo.NoteCount);

        public void UpdateGameLogic(FingerInfo fingerInfo)
        {
            if (fingerInfo.NoteBase == default) return;
            if (!notesManager.AliveNoteList.AsValueEnumerable().Any(x => x == fingerInfo.NoteBase)) return;

            var noteType = fingerInfo.NoteBase.NoteData.NoteType.EnumEquals(ENoteType.Flick);
            if (fingerInfo.IsMissed)
            {
                notesManager.TryRemoveNote(fingerInfo.NoteBase);
                comboController.UpdateCombo(fingerInfo.JudgmentType);
                judgmentCounter.AddJudgmentCount(fingerInfo.JudgmentType, (fingerInfo.MissInfo?.missLongNote ?? false) ? 2 : 1);
                scoreController.AddScore(fingerInfo.JudgmentType);
            }
            else
            {
                if (fingerInfo.NoteBase.NoteData.NoteType.EnumEquals(ENoteType.Flick) && fingerInfo.FingerType.EnumEquals(EFingerType.Down)) return;
                comboController.UpdateCombo(fingerInfo.JudgmentType);
                judgmentCounter.AddJudgmentCount(fingerInfo.JudgmentType, 1);
                scoreController.AddScore(fingerInfo.JudgmentType);
                if (fingerInfo.NoteBase.NoteData.NoteType.EnumEquals(ENoteType.Long) && fingerInfo.FingerType.EnumEquals(EFingerType.Down)) return;
                notesManager.TryRemoveNote(fingerInfo.NoteBase);
            }
            if (notesManager.SongOption.IsAuto) hpBarController.UpdateHp(fingerInfo.JudgmentType, notesManager.LoadedChartInfo.NoteCount);
        }

        public IDisposable SubscribeHpUpdate(Func<float, bool> predicate, Action<float> onUpdate) => hpBarController.EveryUpdateHpPercentAsAsyncEnumerable.TakeWhile(x => predicate(x)).Subscribe(onUpdate);
        public IDisposable SubscribeComboUpdate(Func<float, bool> predicate, Action<int> onUpdate) => comboController.EveryUpdateComboAsAsyncEnumerable.TakeWhile(x => predicate(x)).Subscribe(x => onUpdate(x));
        public IDisposable SubscribeScoreUpdate(Action<int> onUpdate) => scoreController.EveryUpdateScoreAsAsyncEnumerable.Select(x => (int)x).Subscribe(onUpdate);
    }
}