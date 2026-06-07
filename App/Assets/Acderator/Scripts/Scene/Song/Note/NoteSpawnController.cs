#nullable enable

using R3;
using Zenject;

namespace Song
{
    public class NoteSpawnController
    {
        [Inject] private readonly NotesManager notesManager = default!;

        private float laneLength;
        private int nextSpawnIndex;

        private readonly Subject<NoteData> noteFactorySubject = new();
        public Observable<NoteData> NoteFactoryAsObservable => noteFactorySubject;

        public void Init(float laneLength) => this.laneLength = laneLength;

        public void UpdateSpawn()
        {
            var noteDataList = notesManager.LoadedChartInfo?.NoteDataList;
            if (noteDataList == null) return;

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
