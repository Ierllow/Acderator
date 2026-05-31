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
        [Inject] private readonly MasterDataManager masterDataManager;
        [Inject] private readonly NotesManager notesManager;

        private float? badJudgmentZone;

        private float BadJudgmentZone => badJudgmentZone ??= masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => x.Type <= (int)EJudgementType.Bad).Zone;

        public bool TryJudge(NoteBase note, EFingerType fingerType, bool allowMiss, out EJudgementType judgementType)
        {
            judgementType = EJudgementType.None;

            var noteData = notesManager.GetNoteData(note);
            judgementType = GetJudgmentType(GetNoteDiffSec(fingerType, noteData));
            if (judgementType != EJudgementType.None) return true;

            if (!allowMiss) return false;
            judgementType = EJudgementType.Miss;
            return true;
        }

        private EJudgementType GetJudgmentType(float diffSec)
        {
            var zone = masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => diffSec <= x.Zone);
            return zone == default ? EJudgementType.None : (EJudgementType)zone.Type;
        }

        private float GetNoteDiffSec(EFingerType fingerType, NoteData noteData) => noteData.NoteType == ENoteType.Curve ? GetCurveNoteDiffSec(noteData) : GetDiffSec(fingerType, noteData);

        private float GetDiffSec(EFingerType fingerType, NoteData noteData)
        {
            var noteSec = fingerType == EFingerType.Down ? noteData.SecBegin : noteData.SecEnd;
            return Math.Abs(noteSec - notesManager.CurrentSec + notesManager.SongOption.TapTiming * 0.1f);
        }

        private float GetCurveNoteDiffSec(NoteData noteData)
        {
            var curveProgress = noteData.CurveDuration > 0 ? Mathf.Clamp01((notesManager.CurrentSec - noteData.SecBegin) / noteData.CurveDuration) : 1f;
            var curveTime = noteData.SecBegin + noteData.CurveDuration * curveProgress;
            return Math.Abs(curveTime - notesManager.CurrentSec + notesManager.SongOption.TapTiming * 0.1f);
        }

        public bool IsJustAutoTiming(NoteBase note, EFingerType fingerType)
        {
            var data = notesManager.GetNoteData(note);
            var sec = notesManager.CurrentSec;
            var beginPassed = data.SecBegin <= sec;
            return data.NoteType switch
            {
                ENoteType.Single => beginPassed,
                ENoteType.Flick => fingerType == EFingerType.Down ? beginPassed && !note.IsTapping : note.IsTapping,
                ENoteType.Long => fingerType == EFingerType.Down ? beginPassed && !note.IsTapping : data.SecEnd <= sec && note.IsTapping,
                ENoteType.Curve => fingerType == EFingerType.Down ? beginPassed && !note.IsTapping : data.SecBegin + data.CurveDuration <= sec && note.IsTapping,
                _ => false,
            };
        }

        public bool IsMissed(NoteBase note, float currentSec, out bool missEnd)
        {
            missEnd = false;
            var data = notesManager.GetNoteData(note);

            var beginMissed = data.SecBegin - currentSec < -BadJudgmentZone;
            return data.NoteType switch
            {
                ENoteType.Single => beginMissed && !note.IsTapping,
                ENoteType.Flick => IsFlickMissed(note, data, currentSec, beginMissed),
                ENoteType.Long or ENoteType.Curve => IsHoldMissed(note, data, currentSec, beginMissed, out missEnd),
                _ => false,
            };
        }

        private bool IsFlickMissed(NoteBase note, NoteData data, float currentSec, bool beginMissed) => !note.IsTapping ? beginMissed : data.SecEnd - currentSec < -BadJudgmentZone;

        private bool IsHoldMissed(NoteBase note, NoteData data, float currentSec, bool beginMissed, out bool missEnd)
        {
            missEnd = false;
            if (!note.IsTapping)
            {
                missEnd = beginMissed;
                return beginMissed;
            }

            var duration = GetHoldDuration(data);
            var endMissed = data.SecBegin + duration - currentSec < -BadJudgmentZone;
            return beginMissed || endMissed;
        }

        private float GetHoldDuration(NoteData data) => data.NoteType switch
        {
            ENoteType.Curve => data.CurveDuration,
            _ => data.SecEnd - data.SecBegin,
        };
    }
}