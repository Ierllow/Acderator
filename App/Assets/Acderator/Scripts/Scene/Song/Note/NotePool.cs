using System;
using UnityEngine.Pool;

namespace Song
{
    public class NotePool<TNote> : ObjectPool<TNote> where TNote : NoteBase
    {
        public NotePool(Func<TNote> createFunc, Action<TNote> actionOnGet = null, Action<TNote> actionOnRelease = null, Action<TNote> actionOnDestroy = null, bool collectionCheck = false, int defaultCapacity = 5, int maxSize = 10)
            : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize) { }
    }

    public static class NotePoolExtensions
    {
        public static bool TryGetPool<T>(this T[,] array, int index1, int index2, out T note) where T : class
        {
            note = index1 < 0 || index1 >= array.GetLength(0) ? null : index2 < 0 || index2 >= array.GetLength(1) ? null : array[index1, index2];
            return note != default(T);
        }
    }
}