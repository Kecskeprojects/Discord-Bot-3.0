using DatabaseORM.Resources;

namespace DatabaseORM.Caching;

internal sealed class ServerCache
{
    private SizedDictionary<ulong, ServerResource> Cache { get; } = new(50);

    public void RemoveCachedEntityManually(ulong key)
    {
        _ = Cache.Remove(key);
    }

    public void ClearCachedEntityManually()
    {
        Cache.Clear();
    }

    //Null when the server is not cached
    public ServerResource? TryGetValue(ulong key)
    {
        return Cache.TryGetValue(key, out ServerResource? server) ? server : null;
    }

    public void TryAddValue(ulong discordId, ServerResource result)
    {
        if (Cache.ContainsKey(discordId))
        {
            Cache[discordId] = result;
        }
        _ = Cache.TryAdd(discordId, result);
    }
}
