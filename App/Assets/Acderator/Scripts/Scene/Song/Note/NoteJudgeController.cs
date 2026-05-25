using Intense;
using Intense.Master;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Song
{
    public sealed class NoteJudgeController
    {
        [Inject] private MasterDataManager masterDataManager;
        [Inject] private NotesManager notesManager;

        private float? badJudgmentZone;

        private float BadJudgmentZone => badJudgmentZone ??= masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => x.Type <= (int)EJudgementType.Bad).Zone;

        public EJudgementType JudgeOrMiss(NoteBase note, EFingerType fingerType)
        {
            var judgementType = GetJudgmentType(GetNoteDiffSec(fingerType, note.NoteData));
            return judgementType != EJudgementType.None ? judgementType : EJudgementType.Miss;
        }

        public EJudgementType GetJudgmentType(float diffSec)
        {
            var zone = masterDataManager.MemoryDatabase.SongJudgeZoneMasterTable.FirstOrDefault(x => diffSec <= x.Zone);
            return zone == default ? EJudgementType.None : (EJudgementType)zone.Type;
        }

        public float GetNoteDiffSec(EFingerType fingerType, NoteData noteData)
        {
            if (noteData.NoteType == ENoteType.Curve)
            {
                var curveProgress = Mathf.Clamp01((notesManager.CurrentSec - noteData.SecBegin) / noteData.CurveDuration);
                return notesManager.GetCurveNoteDiffSec(noteData, curveProgress);
            }
            return notesManager.GetDiffSec(fingerType, noteData);
        }

        public bool IsJustAutoTiming(NoteBase note, EFingerType fingerType)
        {
            var data = note.NoteData;
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
            var data = note.NoteData;
            if (data.NoteType is not (ENoteType.Long or ENoteType.Curve)) return false;

            var beginMissed = data.SecBegin - currentSec < -BadJudgmentZone;

            if (!note.IsTapping)
            {
                if (!beginMissed) return false;
                missEnd = true;
                return true;
            }

            var duration = data.NoteType == ENoteType.Curve ? data.CurveDuration : data.SecEnd - data.SecBegin;
            var endMissed = data.SecBegin + duration - currentSec < -BadJudgmentZone;
            return beginMissed || endMissed;
        }
    }
}
