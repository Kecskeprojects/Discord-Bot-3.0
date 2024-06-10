using Discord_Bot.Resources;

namespace Discord_Bot.Core.Caching;

public class ServerCache
{
    //Todo: Revisit method of caching, add functions to Service or Command level Base class to clear cache, possibly make a more universal solution
    private SizedDictionary<ulong, ServerResource> Cache { get; } = new(50);

    public void RemoveCachedEntityManually(ulong key)
    {
        if (Cache.ContainsKey(key))
        {
            Cache.Remove(key);
        }
    }

    public void ClearCachedEntityManually()
    {
        Cache.Clear();
    }

    public ServerResource TryGetValue(ulong key)
    {
        return Cache.TryGetValue(key, out ServerResource server) ? server : null;
    }

    public void TryAddValue(ulong discordId, ServerResource result)
    {
        if (Cache.ContainsKey(discordId))
        {
            Cache[discordId] = result;
        }
        Cache.TryAdd(discordId, result);
    }
}
