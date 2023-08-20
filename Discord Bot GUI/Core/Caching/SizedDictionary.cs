using System.Collections.Generic;

namespace Discord_Bot.Core.Caching
{
    public sealed class SizedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {

        private readonly int maxSize;
        private Queue<TKey> keys;

        public SizedDictionary(int size)
        {
            maxSize = size;
            keys = new Queue<TKey>();
        }

        new public void Add(TKey key, TValue value)
        {
            if (key == null) throw new();
            base.Add(key, value);
            keys.Enqueue(key);
            if (keys.Count > maxSize) base.Remove(keys.Dequeue());
        }

        new public bool Remove(TKey key)
        {
            if (key == null) throw new();
            if (!keys.Contains(key)) return false;
            var newQueue = new Queue<TKey>();
            while (keys.Count > 0)
            {
                var thisKey = keys.Dequeue();
                if (!thisKey.Equals(key)) newQueue.Enqueue(thisKey);
            }
            keys = newQueue;
            return base.Remove(key);
        }
    }
}
