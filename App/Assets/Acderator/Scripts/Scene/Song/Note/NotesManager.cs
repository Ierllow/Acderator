using Intense.Master;
using R3;
using System;
using System.Collections.Generic;
using ZLinq;

namespace Song
{
    public class NotesManager
    {
        public float CurrentSec { get; private set; } = default;
        public float CurrentBeat { get; private set; } = default;
        public List<NoteBase> AliveNoteList { get; } = new();
        public LoadedChartInfo LoadedChartInfo { get; private set; } = default;
        public List<NoteSpeedChange> NoteSpeedChangeList { get; } = new();
        public SongOption SongOption { get; private set; } = default;
        public float CurrentNoteSpeed { get; private set; } = default;

        public const int MIN_NOTES_SPEED = 1;

        private int currentSpeedChangeIndex = 0;
        private readonly List<NoteBase>[] aliveNotesByLaneList = { new(), new(), new(), new(), };

        public NotesManager(SongOption songOption) => SongOption = songOption;

        public void Init(LoadedChartInfo loadedChartInfo)
        {
            CurrentNoteSpeed = SongOption.NoteSpeed;
            LoadedChartInfo = loadedChartInfo;
            NoteSpeedChangeList.AddRange(loadedChartInfo.HeaderData.NoteSpeedChangeList);
            UpdateNoteSpeed();
        }

        public void AddAliveNote(NoteBase note)
        {
            AliveNoteList.Add(note);
            aliveNotesByLaneList[note.NoteData.Lane].Add(note);
        }

        public bool TryRemoveNote(NoteBase note)
        {
            var removed = AliveNoteList.Remove(note);
            aliveNotesByLaneList[note.NoteData.Lane].Remove(note);
            return removed;
        }

        public void UpdateBeat(float sec)
        {
            CurrentSec = sec;
            CurrentBeat = LoadedChartInfo != default ? sec * ((LoadedChartInfo.HeaderData?.Tempo ?? 0) / 60f) : 0;
        }

        public float GetDiffSec(EFingerType fingerType, NoteData noteData)
        {
            var noteSec = fingerType.EnumEquals(EFingerType.Down) ? noteData.SecBegin : noteData.SecEnd;
            return Math.Abs(noteSec - CurrentSec + SongOption.TapTiming * 0.1f);
        }

        public float GetCurveNoteDiffSec(NoteData noteData, float curveProgress)
        {
            if (noteData.NoteType.EnumEquals(ENoteType.Curve))
            {
                var curveTime = noteData.SecBegin + noteData.CurveDuration * curveProgress;
                return Math.Abs(curveTime - CurrentSec + SongOption.TapTiming * 0.1f);
            }
            return default;
        }

        public bool TryGetNote(EFingerType type, int lane, out NoteBase note)
        {
            var laneNoteList = aliveNotesByLaneList[lane];
            var aliveNotes = laneNoteList.AsValueEnumerable().Where(x => x != null && x.IsActive);
            note = (type.EnumEquals(EFingerType.Down) ? aliveNotes.Where(x => !x.IsTapping) : aliveNotes.Where(x => x.IsTapping))
                .OrderBy(x => Math.Abs(x.NoteData.BeatBegin - CurrentBeat))
                .FirstOrDefault();

            return note != null;
        }

        public bool TryGetFlickNote(int lane, out NoteBase note)
        {
            note = aliveNotesByLaneList[lane]
                .AsValueEnumerable()
                .Where(x => x != null && x.IsActive && x.NoteData.NoteType.EnumEquals(ENoteType.Flick))
                .OrderBy(x => Math.Abs(x.NoteData.BeatBegin - CurrentBeat))
                .FirstOrDefault();

            return note != null;
        }

        public void UpdateNoteSpeed()
        {
            while (currentSpeedChangeIndex < NoteSpeedChangeList.AsValueEnumerable().Count() && CurrentBeat >= NoteSpeedChangeList.AsValueEnumerable().ElementAt(currentSpeedChangeIndex).Beat)
            {
                var speedChange = NoteSpeedChangeList.AsValueEnumerable().ElementAt(currentSpeedChangeIndex);
                var diffNoteSpeed = SongOption.NoteSpeed - MIN_NOTES_SPEED;
                CurrentNoteSpeed = (float)speedChange.Speed + diffNoteSpeed;
                currentSpeedChangeIndex++;
            }
        }

        public float GetSpawnOffset(float laneLength)
        {
            var bpm = LoadedChartInfo.HeaderData.Tempo;
            var speed = CurrentNoteSpeed;
            var speedSec = speed * (bpm / 60f);
            return laneLength / speedSec;
        }
    }
}