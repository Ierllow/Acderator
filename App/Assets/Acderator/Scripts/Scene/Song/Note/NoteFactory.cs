#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Song
{
    public class NoteFactory : MonoBehaviour
    {
        [Serializable]
        private class NotePrefabs
        {
            public SingleNote singleNote = default!;
            public LongNote longNote = default!;
            public FlickNote flickNote = default!;
            public CurveNote curveNote = default!;
        }

        [SerializeField] private Transform[] parents = default!;
        [SerializeField] private NotePrefabs notePrefabs = default!;

        private ObjectPool<NoteBase>[,]? pools;
        private readonly Dictionary<NoteBase, NoteData> noteDict = new();

        public IReadOnlyList<Transform> LaneParents => parents;

        public void Init()
        {
            var laneCount = parents.Length;
            var typeCount = Enum.GetValues(typeof(ENoteType)).Length;
            pools = new ObjectPool<NoteBase>[typeCount, laneCount];

            foreach (var (parent, lane) in parents.Select((parent, lane) => (parent, lane)))
            {
                pools[(int)ENoteType.Single, lane] = CreatePool<NoteBase>(notePrefabs.singleNote, parent);
                pools[(int)ENoteType.Long, lane] = CreatePool<NoteBase>(notePrefabs.longNote, parent);
                pools[(int)ENoteType.Flick, lane] = CreatePool<NoteBase>(notePrefabs.flickNote, parent);
                pools[(int)ENoteType.Curve, lane] = CreatePool<NoteBase>(notePrefabs.curveNote, parent);
            }
        }

        private ObjectPool<T> CreatePool<T>(T prefab, Transform parent) where T : NoteBase => new(
            createFunc: () =>
            {
                var note = Instantiate(prefab, parent);
                note.Finalized = ReleaseNote;
                return note;
            },
            actionOnGet: note => note.gameObject.SetActive(true),
            actionOnRelease: note => note.gameObject.SetActive(false),
            actionOnDestroy: note =>
            {
                noteDict.Remove(note);
                Destroy(note.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: 5,
            maxSize: 10
        );

        public NoteBase? SpawnNote(NoteData noteData)
        {
            if (pools == null) return null;

            var type = (int)noteData.NoteType;
            var lane = noteData.Lane;
            if (type < 0 || type >= pools.GetLength(0) || lane < 0 || lane >= pools.GetLength(1)) return null;

            var pool = pools[type, lane];
            if (pool == null) return null;

            var note = pool.Get();
            noteDict[note] = noteData;
            note.Init(noteData);
            return note;
        }

        private void ReleaseNote(NoteBase note)
        {
            if (pools == null || !noteDict.TryGetValue(note, out var data)) return;

            var type = (int)data.NoteType;
            var lane = data.Lane;
            if (type < 0 || type >= pools.GetLength(0) || lane < 0 || lane >= pools.GetLength(1)) return;

            pools[type, lane]?.Release(note);
        }
    }
}
