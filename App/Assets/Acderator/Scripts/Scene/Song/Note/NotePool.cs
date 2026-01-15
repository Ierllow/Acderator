using Zenject;

namespace Song
{
    public class NotePool<TNote> : MonoMemoryPool<NoteData, TNote> where TNote : NoteBase
    {
        protected override void Reinitialize(NoteData noteData, TNote note) => note.OnSpawned(noteData, this);
    }
}