namespace Discord_Bot.Core;

/// <summary>
/// The YouTube Data API keys that still have quota today, with the quota units each one has used.
/// <see cref="Reset"/> fills it from the configuration at startup and every day at 08:00 UTC, after YouTube's own reset.
/// </summary>
public sealed class YoutubeKeyStore
{
    private readonly Lock keyLock = new();
    //In the configuration's order, which decides between keys that used the same amount
    private readonly OrderedDictionary<string, int> usedQuota = [];

    //The key that used the fewest units today, so the requests spread over every key; null when every key's quota is used up.
    //Until 2026-11-24 one key was picked at random and used until its quota ran out, and the units were counted but never read
    public string? CurrentKey
    {
        get
        {
            lock (keyLock)
            {
                return LeastUsedKey();
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
        }
    }

    //Counts the quota units a request used; a key that was dropped meanwhile is not added back
    public void AddUsage(string key, int units)
    {
        lock (keyLock)
        {
            if (usedQuota.TryGetValue(key, out int used))
            {
                usedQuota[key] = used + units;
            }
        }
    }

    //Drops a key whose quota ran out, and returns the key to use now (null when none are left)
    public string? ReplaceExhaustedKey(string exhaustedKey)
    {
        lock (keyLock)
        {
            _ = usedQuota.Remove(exhaustedKey);
            return LeastUsedKey();
        }
    }

    private string? LeastUsedKey()
    {
        return usedQuota.Count == 0 ? null : usedQuota.MinBy(entry => entry.Value).Key;
    }
}
