namespace Discord_Bot.Core;

/// <summary>
/// The YouTube Data API keys that still have quota today, and the one in use.
/// <see cref="Reset"/> fills it from the configuration at startup and every day at 08:00 UTC, after YouTube's own reset.
/// </summary>
public sealed class YoutubeKeyStore
{
    private readonly Lock keyLock = new();
    private readonly Dictionary<string, int> usedQuota = [];
    private string? currentKey;

    //Null when every key's quota is used up
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
            currentKey = PickKey();
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
            if (currentKey == exhaustedKey)
            {
                currentKey = PickKey();
            }
            return currentKey;
        }
    }

    private string? PickKey()
    {
        return usedQuota.Count == 0 ? null : usedQuota.Keys.ElementAt(Random.Shared.Next(usedQuota.Count));
    }
}
