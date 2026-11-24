namespace Discord_Bot.Core;

/// <summary>
/// The YouTube Data API keys that still have quota today, with the quota units each one has used, and the one in use.
/// <see cref="Reset"/> fills it from the configuration at startup and every day at 08:00 UTC, after YouTube's own reset.
/// </summary>
public sealed class YoutubeKeyStore
{
    //YouTube's default daily quota per key
    private const int DailyQuota = 10_000;

    private readonly Lock keyLock = new();
    //In the configuration's order, which is the order the keys are used in
    private readonly OrderedDictionary<string, int> usedQuota = [];
    private string? currentKey;

    //One key is used until YouTube says its quota ran out or its counted units reach the daily quota; null when every key is used up.
    //Until 2026-11-24 the units were counted but never read, and the next key was picked at random
    public string? CurrentKey
    {
        get
        {
            lock (keyLock)
            {
                return currentKey;
            }
        }
    }

    public void Reset(IEnumerable<string> keys)
    {
        lock (keyLock)
        {
            usedQuota.Clear();
            foreach (string key in keys)
            {
                _ = usedQuota.TryAdd(key, 0);
            }
            currentKey = FirstKeyLeft();
        }
    }

    //Counts the quota units a request used, and drops the key once they reach the daily quota; a key that was dropped meanwhile is not added back
    public void AddUsage(string key, int units)
    {
        lock (keyLock)
        {
            if (!usedQuota.TryGetValue(key, out int used))
            {
                return;
            }

            if (used + units < DailyQuota)
            {
                usedQuota[key] = used + units;
            }
            else
            {
                Drop(key);
            }
        }
    }

    //Drops a key whose quota ran out, and returns the key to use now (null when none are left)
    public string? ReplaceExhaustedKey(string exhaustedKey)
    {
        lock (keyLock)
        {
            Drop(exhaustedKey);
            return currentKey;
        }
    }

    private void Drop(string key)
    {
        _ = usedQuota.Remove(key);
        if (currentKey == key)
        {
            currentKey = FirstKeyLeft();
        }
    }

    private string? FirstKeyLeft()
    {
        return usedQuota.Count == 0 ? null : usedQuota.GetAt(0).Key;
    }
}
