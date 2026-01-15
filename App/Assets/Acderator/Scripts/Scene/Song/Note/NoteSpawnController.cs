using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

namespace Song
{
    public class NoteSpawnController : MonoBehaviour, IController
    {
        [SerializeField] private float spawnTiming = 10f;

        private readonly List<NoteData> noteDataList = new();
        private readonly List<NoteData> spawnedNoteDataList = new();

        private float offset = 0f;

        public Subject<NoteData> NoteFactorySubject { get; } = new();

        public void Init(List<NoteData> noteDataList, float offset)
        {
            this.offset = offset;
            this.noteDataList.AddRange(noteDataList);
        }

        public void UpdateSpawn(float currentSec)
        {
            var swapnNoteList = noteDataList.AsValueEnumerable().Where(note => !spawnedNoteDataList.AsValueEnumerable().Any(x => x == note) && note.SecBegin - offset <= currentSec).ToList();
            foreach (var noteData in swapnNoteList)
            {
                NoteFactorySubject.OnNext(noteData);
                spawnedNoteDataList.Add(noteData);
            }
        }
    }
}