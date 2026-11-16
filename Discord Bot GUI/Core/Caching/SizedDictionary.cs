using Discord_Bot.Tools.NativeTools;
using System;
using System.Collections.Generic;

namespace Discord_Bot.Core.Caching;

public sealed class SizedDictionary<TKey, TValue>(int size) : Dictionary<TKey, TValue> where TKey : notnull
{

    private readonly int maxSize = size;
    private Queue<TKey> keys = new();

    public new bool TryAdd(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (base.TryAdd(key, value))
        {
            keys.Enqueue(key);
            if (keys.Count > maxSize)
            {
                _ = base.Remove(keys.Dequeue());
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public new void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        base.Add(key, value);
        keys.Enqueue(key);
        if (keys.Count > maxSize)
        {
            _ = base.Remove(keys.Dequeue());
        }
    }

    public new bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!keys.Contains(key))
        {
            return false;
        }

        Queue<TKey> newQueue = new();
        while (!CollectionTools.IsNullOrEmpty(keys))
        {
            TKey thisKey = keys.Dequeue();
            if (!thisKey.Equals(key))
            {
                newQueue.Enqueue(thisKey);
            }
        }
        keys = newQueue;
        return base.Remove(key);
    }
    public new void Clear()
    {
        keys.Clear();
        base.Clear();
    }
}
