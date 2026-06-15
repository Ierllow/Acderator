#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Song
{
    internal enum NoteSearchMode { Down, Up, Flick }

    public class NotesManager
    {
        public const int LaneCount = 4;
        public const int MIN_NOTES_SPEED = 1;

        public float CurrentSec { get; private set; } = default;
        public float CurrentBeat { get; private set; } = default;
        public LoadedChartInfo? LoadedChartInfo { get; private set; }
        public List<NoteSpeedChange> NoteSpeedChangeList { get; } = new();
        public SongOption SongOption { get; }
        public float CurrentNoteSpeed { get; private set; } = default;

        public IReadOnlyList<NoteBase> AliveNoteList
        {
            get
            {
                if (aliveNotesCache)
                {
                    allAliveNotesCache.Clear();
                    for (var lane = 0; lane < LaneCount; lane++)
                    {
                        allAliveNotesCache.AddRange(aliveNotesByLaneDict[lane]);
                    }
                    aliveNotesCache = false;
                }
                return allAliveNotesCache;
            }
        }

        private readonly Dictionary<int, List<NoteBase>> aliveNotesByLaneDict = new();
        private readonly Dictionary<NoteBase, NoteData> noteDataDict = new();
        private readonly List<NoteBase> allAliveNotesCache = new();
        private bool aliveNotesCache;
        private int currentSpeedChangeIndex = 0;

        public NotesManager(SongOption songOption)
        {
            SongOption = songOption;
            CurrentNoteSpeed = songOption.NoteSpeed;
        }

        public void Init(LoadedChartInfo loadedChartInfo)
        {
            LoadedChartInfo = loadedChartInfo;
            NoteSpeedChangeList.AddRange(loadedChartInfo.HeaderData.NoteSpeedChangeList);
        }

        public void AddAliveNote(NoteBase note, NoteData data)
        {
            noteDataDict[note] = data;
            aliveNotesByLaneDict[data.Lane].Add(note);
            aliveNotesCache = true;
        }

        public bool RemoveNote(NoteBase note)
        {
            if (!noteDataDict.TryGetValue(note, out var data)) return false;
            var removed = aliveNotesByLaneDict[data.Lane].Remove(note);
            if (removed)
            {
                aliveNotesCache = true;
                noteDataDict.Remove(note);
            }
            return removed;
        }

        public NoteData GetNoteData(NoteBase note) => noteDataDict[note];

        public void UpdateBeat(float sec)
        {
            CurrentSec = sec;
            CurrentBeat = sec * ((LoadedChartInfo?.HeaderData.Tempo ?? 0) / 60f);
        }

        public bool TryGetNote(EFingerType type, int lane, [NotNullWhen(true)] out NoteBase? note) => TryGetNearestNote(lane, type switch
        {
            EFingerType.Down => NoteSearchMode.Down,
            _ => NoteSearchMode.Up,
        }, out note);

        public bool TryGetFlickNote(int lane, [NotNullWhen(true)] out NoteBase? note) => TryGetNearestNote(lane, NoteSearchMode.Flick, out note);

        private bool TryGetNearestNote(int lane, NoteSearchMode mode, [NotNullWhen(true)] out NoteBase? note)
        {
            note = null;
            if (lane < 0 || lane >= LaneCount) return false;

            var laneNoteList = aliveNotesByLaneDict[lane];
            var bestDiff = float.MaxValue;
            NoteBase? bestNote = null;

            for (var i = 0; i < laneNoteList.Count; i++)
            {
                var candidate = laneNoteList[i];
                if (candidate == null || !candidate.IsActive) continue;
                if (!noteDataDict.TryGetValue(candidate, out var candidateData)) continue;
                if (mode switch
                {
                    NoteSearchMode.Down => candidate.IsTapping,
                    NoteSearchMode.Up => !candidate.IsTapping || candidateData.NoteType is not (ENoteType.Long or ENoteType.Curve),
                    NoteSearchMode.Flick => !candidate.IsTapping || candidateData.NoteType != ENoteType.Flick,
                    _ => true,
                }) continue;

                var diff = GetSearchDiffSec(candidateData, mode);
                if (diff >= bestDiff) continue;

                bestDiff = diff;
                bestNote = candidate;
            }

            note = bestNote;
            return note != null;
        }

        private float GetSearchDiffSec(NoteData noteData, NoteSearchMode mode) => Math.Abs(mode switch
        {
            NoteSearchMode.Up => GetNoteEndSec(noteData),
            _ => noteData.SecBegin,
        } - CurrentSec + SongOption.TapTiming * 0.1f);

        private float GetNoteEndSec(NoteData noteData) => noteData.NoteType switch
        {
            ENoteType.Curve => noteData.SecBegin + noteData.CurveDuration,
            _ => noteData.SecEnd,
        };

        public void UpdateNoteSpeed()
        {
            while (currentSpeedChangeIndex < NoteSpeedChangeList.Count && CurrentBeat >= NoteSpeedChangeList[currentSpeedChangeIndex].Beat)
            {
                var diffNoteSpeed = SongOption.NoteSpeed - MIN_NOTES_SPEED;
                CurrentNoteSpeed = (float)NoteSpeedChangeList[currentSpeedChangeIndex].Speed + diffNoteSpeed;
                currentSpeedChangeIndex++;
            }
        }

        public bool ShouldSpawn(NoteData noteData, float laneLength)
        {
            if (CurrentNoteSpeed <= 0) return noteData.SecBegin <= CurrentSec;

            var notePositionY = (noteData.BeatBegin - CurrentBeat) * CurrentNoteSpeed;
            return notePositionY <= laneLength;
        }

        public float GetInitialSpawnLeadInSec(float laneLength)
        {
            var bpm = LoadedChartInfo?.HeaderData.Tempo ?? 0;
            var speed = CurrentNoteSpeed;
            var speedSec = speed * (bpm / 60f);
            return speedSec <= 0 ? 0f : laneLength / speedSec;
        }
    }
}