using System;
using UnityEngine;
using Zenject;
using ZLinq;

namespace Song
{
    public class NoteFactory : MonoBehaviour
    {
        [SerializeField] private Transform[] parents;

        [Inject] private NotePool<SingleNote> singlePool;
        [Inject] private NotePool<LongNote> longPool;
        [Inject] private NotePool<FlickNote> flickPool;
        [Inject] private NotePool<CurveNote> curvePool;

        public NoteBase SpawnNote(NoteData noteData)
        {
            NoteBase note = noteData.NoteType switch
            {
                ENoteType.Single => singlePool.Spawn(noteData),
                ENoteType.Long => longPool.Spawn(noteData),
                ENoteType.Flick => flickPool.Spawn(noteData),
                ENoteType.Curve => curvePool.Spawn(noteData),
                _ => throw new NotImplementedException()
            };
            note.transform.SetParent(parents.AsValueEnumerable().ElementAt(noteData.Lane), false);

            return note;
        }
    }
}