using System;
using UnityEngine;

namespace Song
{
    public class NoteFactory : MonoBehaviour
    {
        [Serializable]
        private class NotePrefabs
        {
            public SingleNote singleNote;
            public LongNote longNote;
            public FlickNote flickNote;
            public CurveNote curveNote;
        }

        [SerializeField] private Transform[] parents;
        [SerializeField] private NotePrefabs notePrefabs;

        private NotePool<NoteBase>[,] pools;

        public void Init()
        {
            var laneCount = parents.Length;
            var typeCount = Enum.GetValues(typeof(ENoteType)).Length;
            pools = new NotePool<NoteBase>[typeCount, laneCount];

            for (var lane = 0; lane < laneCount; lane++)
            {
                var parent = parents[lane];
                pools[(int)ENoteType.Single, lane] = CreatePool<NoteBase>(notePrefabs.singleNote, parent);
                pools[(int)ENoteType.Long, lane] = CreatePool<NoteBase>(notePrefabs.longNote, parent);
                pools[(int)ENoteType.Flick, lane] = CreatePool<NoteBase>(notePrefabs.flickNote, parent);
                pools[(int)ENoteType.Curve, lane] = CreatePool<NoteBase>(notePrefabs.curveNote, parent);
            }
        }

        private NotePool<T> CreatePool<T>(T prefab, Transform parent) where T : NoteBase => new(
            createFunc: () => Instantiate(prefab, parent),
            actionOnGet: note => note.gameObject.SetActive(true),
            actionOnRelease: note => note.gameObject.SetActive(false),
            actionOnDestroy: note => Destroy(note.gameObject),
            collectionCheck: false,
            defaultCapacity: 5,
            maxSize: 10
        );

        public NoteBase SpawnNote(NoteData noteData)
        {
            if (pools?.TryGetPool((int)noteData.NoteType, noteData.Lane, out var pool) ?? false)
            {
                var note = pool.Get();
                note.Init(noteData, pool);
                return note;
            }
            return default;
        }
    }
}