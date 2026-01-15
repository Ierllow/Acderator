namespace System.Collections.Generic
{
    [Serializable]
    public sealed class KeyAndValue<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }
}