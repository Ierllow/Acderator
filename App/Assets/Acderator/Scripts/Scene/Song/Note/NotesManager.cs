using System;
using System.Collections.Generic;

namespace Song
{
    public class NotesManager
    {
        private enum NoteSearchMode { Down, Up, Flick }

        public float CurrentSec { get; private set; } = default;
        public float CurrentBeat { get; private set; } = default;
        public List<NoteBase> AliveNoteList { get; } = new();
        public LoadedChartInfo LoadedChartInfo { get; private set; } = default;
        public List<NoteSpeedChange> NoteSpeedChangeList { get; } = new();
        public SongOption SongOption { get; init; } = default;
        public float CurrentNoteSpeed { get; private set; } = default;

        private int currentSpeedChangeIndex = 0;
        private readonly List<NoteBase>[] aliveNotesByLaneList = { new(), new(), new(), new(), };

        public const int MIN_NOTES_SPEED = 1;

        public NotesManager(SongOption songOption)
        {
            SongOption = songOption;
            CurrentNoteSpeed = songOption.NoteSpeed;
        }

        public void Init(LoadedChartInfo loadedChartInfo)
        {
            LoadedChartInfo = loadedChartInfo;
            NoteSpeedChangeList.AddRange(loadedChartInfo.HeaderData.NoteSpeedChangeList);
            UpdateNoteSpeed();
        }

        public void AddAliveNote(NoteBase note)
        {
            AliveNoteList.Add(note);
            aliveNotesByLaneList[note.NoteData.Lane].Add(note);
        }

        public bool RemoveNote(NoteBase note)
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
            var noteSec =fingerType == EFingerType.Down ? noteData.SecBegin : noteData.SecEnd;
            return Math.Abs(noteSec - CurrentSec + SongOption.TapTiming * 0.1f);
        }

        public float GetCurveNoteDiffSec(NoteData noteData, float curveProgress)
        {
            if (noteData.NoteType == ENoteType.Curve)
            {
                var curveTime = noteData.SecBegin + noteData.CurveDuration * curveProgress;
                return Math.Abs(curveTime - CurrentSec + SongOption.TapTiming * 0.1f);
            }
            return default;
        }

        public bool TryGetNote(EFingerType type, int lane, out NoteBase note)
        {
            var mode =type == EFingerType.Down ? NoteSearchMode.Down : NoteSearchMode.Up;
            return TryGetNearestNote(lane, mode, out note);
        }

        public bool TryGetFlickNote(int lane, out NoteBase note) => TryGetNearestNote(lane, NoteSearchMode.Flick, out note);

        private bool TryGetNearestNote(int lane, NoteSearchMode mode, out NoteBase note)
        {
            var laneNoteList = aliveNotesByLaneList[lane];
            var bestDiff = float.MaxValue;
            var bestNote = default(NoteBase);

            for (var i = 0; i < laneNoteList.Count; i++)
            {
                var candidate = laneNoteList[i];
                if (candidate == null || !candidate.IsActive) continue;

                switch (mode)
                {
                    case NoteSearchMode.Down when candidate.IsTapping:
                    case NoteSearchMode.Up when !candidate.IsTapping:
                        continue;
                    case NoteSearchMode.Flick when candidate.NoteData.NoteType != ENoteType.Flick:
                        continue;
                }

                var diff = Math.Abs(candidate.NoteData.BeatBegin - CurrentBeat);
                if (diff >= bestDiff) continue;

                bestDiff = diff;
                bestNote = candidate;
            }

            note = bestNote;
            return note != null;
        }

        public void UpdateNoteSpeed()
        {
            while (currentSpeedChangeIndex < NoteSpeedChangeList.Count && CurrentBeat >= NoteSpeedChangeList[currentSpeedChangeIndex].Beat)
            {
                var diffNoteSpeed = SongOption.NoteSpeed - MIN_NOTES_SPEED;
                CurrentNoteSpeed = (float)NoteSpeedChangeList[currentSpeedChangeIndex].Speed + diffNoteSpeed;
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