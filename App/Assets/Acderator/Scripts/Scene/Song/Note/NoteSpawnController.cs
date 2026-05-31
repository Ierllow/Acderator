using R3;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Song
{
    public class NoteSpawnController : MonoBehaviour, IController
    {
        [Inject] private readonly NotesManager notesManager;

        private readonly List<NoteData> noteDataList = new();

        private float laneLength;
        private int nextSpawnIndex;

        private readonly Subject<NoteData> noteFactorySubject = new();
        public Observable<NoteData> NoteFactoryAsObservable => noteFactorySubject;

        public void Init(List<NoteData> noteDataList, float laneLength)
        {
            this.laneLength = laneLength;
            this.noteDataList.Clear();
            this.noteDataList.AddRange(noteDataList);
            nextSpawnIndex = 0;
        }

        public void UpdateSpawn()
        {
            while (nextSpawnIndex < noteDataList.Count)
            {
                var noteData = noteDataList[nextSpawnIndex];
                if (!notesManager.ShouldSpawn(noteData, laneLength)) break;

                noteFactorySubject.OnNext(noteData);
                nextSpawnIndex++;
            }
        }
    }
}