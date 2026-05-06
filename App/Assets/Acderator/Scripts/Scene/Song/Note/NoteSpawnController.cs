using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Song
{
    public class NoteSpawnController : MonoBehaviour, IController
    {
        [SerializeField] private float spawnTiming = 10f;

        private readonly List<NoteData> noteDataList = new();
        private readonly List<NoteData> spawnedNoteDataList = new();

        private float offset = 0f;
        private int nextSpawnIndex;

        public Subject<NoteData> NoteFactorySubject { get; } = new();

        public void Init(List<NoteData> noteDataList, float offset)
        {
            this.offset = offset;
            this.noteDataList.AddRange(noteDataList);
        }

        public void UpdateSpawn(float currentSec)
        {
            while (nextSpawnIndex < noteDataList.Count)
            {
                var noteData = noteDataList[nextSpawnIndex];
                if (noteData.SecBegin - offset > currentSec) break;

                NoteFactorySubject.OnNext(noteData);
                nextSpawnIndex++;
            }
        }
    }
}