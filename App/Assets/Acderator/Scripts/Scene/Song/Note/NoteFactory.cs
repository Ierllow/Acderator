#nullable enable

using Intense.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

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

        [HideInInspector, SerializeField] private Transform[] parents = default!;
        [HideInInspector, SerializeField] private NotePrefabs notePrefabs = default!;

        [Inject] private readonly AddressableAssetManager addressableAssetManager = default!;
        [Inject] private readonly AddressablePrefabResolver addressablePrefabResolver = default!;

        private ObjectPool<NoteBase>[,]? pools;
        private readonly Dictionary<NoteBase, NoteData> noteDict = new();

        public IReadOnlyList<Transform> LaneParents => parents;

        public void Init()
        {
            var laneCount = parents.Length;
            var typeCount = Enum.GetValues(typeof(ENoteType)).Length;
            pools = new ObjectPool<NoteBase>[typeCount, laneCount];
            var singleNote = addressablePrefabResolver.GetComponentOrFallback("SingleNote", notePrefabs.singleNote);
            var longNote = addressablePrefabResolver.GetComponentOrFallback("LongNote", notePrefabs.longNote);
            var flickNote = addressablePrefabResolver.GetComponentOrFallback("FlickNote", notePrefabs.flickNote);
            var curveNote = addressablePrefabResolver.GetComponentOrFallback("CurveNote", notePrefabs.curveNote);

            foreach (var (parent, lane) in parents.Select((parent, lane) => (parent, lane)))
            {
                pools[(int)ENoteType.Single, lane] = CreatePoolOrDefault(singleNote, parent)!;
                pools[(int)ENoteType.Long, lane] = CreatePoolOrDefault(longNote, parent)!;
                pools[(int)ENoteType.Flick, lane] = CreatePoolOrDefault(flickNote, parent)!;
                pools[(int)ENoteType.Curve, lane] = CreatePoolOrDefault(curveNote, parent)!;
            }
        }

        private ObjectPool<NoteBase>? CreatePoolOrDefault(NoteBase? prefab, Transform parent) => prefab != null ? CreatePool(prefab, parent) : null;

        private ObjectPool<T> CreatePool<T>(T prefab, Transform parent) where T : NoteBase => new(
            createFunc: () =>
            {
                var note = Instantiate(prefab, parent);
                ResolveSprites(note);
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

        private void ResolveSprites(NoteBase note)
        {
            foreach (var spriteRenderer in note.GetComponentsInChildren<AddressableSpriteRenderer>(true))
            {
                spriteRenderer.Resolve(addressableAssetManager.GetSprite);
            }
        }

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
            if (pools == null || !noteDict.Remove(note, out var data)) return;

            var type = (int)data.NoteType;
            var lane = data.Lane;
            if (type < 0 || type >= pools.GetLength(0) || lane < 0 || lane >= pools.GetLength(1)) return;

            pools[type, lane]?.Release(note);
        }
    }
}