#nullable enable

using Intense;
using Intense.Master;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Song
{
    public sealed class NoteJudgeController
    {
        [Inject] private readonly MasterDataManager masterDataManager = default!;
        [Inject] private readonly NotesManager notesManager = default!;

        private float? badJudgmentZone;
        private float BadJudgmentZone => badJudgmentZone ??= masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.LastOrDefault(x => x.Type <= (int)JudgementType.Bad).Zone;

        public bool TryJudge(NoteBase note, FingerType fingerType, bool allowMiss, out JudgementType judgementType)
        {
            judgementType = JudgementType.None;

            var noteData = notesManager.GetNoteData(note);
            judgementType = GetJudgmentType(GetNoteDiffSec(fingerType, noteData));
            if (judgementType != JudgementType.None) return true;

            if (!allowMiss) return false;
            judgementType = JudgementType.Miss;
            return true;
        }

        private JudgementType GetJudgmentType(float diffSec)
        {
            var zone = masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => diffSec <= x.Zone);
            return zone == default ? JudgementType.None : (JudgementType)zone.Type;
        }

        private float GetNoteDiffSec(FingerType fingerType, NoteData noteData) => (noteData.NoteType, fingerType) switch
        {
            (NoteType.Curve, FingerType.Down) => GetCurveNoteDiffSec(noteData),
            (NoteType.Curve, _) => GetDiffSec(noteData.SecBegin + noteData.CurveDuration),
            _ => GetDiffSec(fingerType == FingerType.Down ? noteData.SecBegin : noteData.SecEnd),
        };

        private float GetDiffSec(float noteSec) => Math.Abs(noteSec - notesManager.CurrentSec + notesManager.SongOption.TapTiming * 0.1f);

        private float GetCurveNoteDiffSec(NoteData noteData)
        {
            var curveProgress = noteData.CurveDuration > 0 ? Mathf.Clamp01((notesManager.CurrentSec - noteData.SecBegin) / noteData.CurveDuration) : 1f;
            var curveTime = noteData.SecBegin + noteData.CurveDuration * curveProgress;
            return Math.Abs(curveTime - notesManager.CurrentSec + notesManager.SongOption.TapTiming * 0.1f);
        }

        public bool IsJustAutoTiming(NoteBase note, FingerType fingerType)
        {
            var data = notesManager.GetNoteData(note);
            var sec = notesManager.CurrentSec;
            var beginPassed = data.SecBegin <= sec;
            return data.NoteType switch
            {
                NoteType.Single => beginPassed,
                NoteType.Flick => fingerType == FingerType.Down ? beginPassed && !note.IsTapping : note.IsTapping,
                NoteType.Long => fingerType == FingerType.Down ? beginPassed && !note.IsTapping : data.SecEnd <= sec && note.IsTapping,
                NoteType.Curve => fingerType == FingerType.Down ? beginPassed && !note.IsTapping : data.SecBegin + data.CurveDuration <= sec && note.IsTapping,
                _ => false,
            };
        }

        public bool IsMissed(NoteBase note, float currentSec)
        {
            var data = notesManager.GetNoteData(note);
            var beginMissed = data.SecBegin - currentSec < -BadJudgmentZone;
            return data.NoteType switch
            {
                NoteType.Single => beginMissed && !note.IsTapping,
                NoteType.Flick => IsFlickMissed(note, data, currentSec, beginMissed),
                NoteType.Long or NoteType.Curve => IsHoldMissed(note, data, currentSec, beginMissed),
                _ => false,
            };
        }

        private bool IsFlickMissed(NoteBase note, NoteData data, float currentSec, bool beginMissed) => !note.IsTapping ? beginMissed : data.SecEnd - currentSec < -BadJudgmentZone;

        private bool IsHoldMissed(NoteBase note, NoteData data, float currentSec, bool beginMissed) => !note.IsTapping ? beginMissed : data.SecBegin + GetHoldDuration(data) - currentSec < -BadJudgmentZone;

        private float GetHoldDuration(NoteData data) => data.NoteType switch
        {
            NoteType.Curve => data.CurveDuration,
            _ => data.SecEnd - data.SecBegin,
        };
    }
}