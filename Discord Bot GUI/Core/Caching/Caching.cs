using Discord_Bot.Resources;

namespace Discord_Bot.Core.Caching
{
    public class Cache
    {
        //Todo: Revisit method of caching, add functions to Service or Command level Base class to clear cache, possibly make a more universal solution
        private SizedDictionary<ulong, ServerResource> ServerCache { get; } = new(10);

        public void RemoveCachedEntityManually(ulong key)
        {
            if (ServerCache.ContainsKey(key))
            {
                ServerCache.Remove(key);
            }
        }

        public void ClearCachedEntityManually()
        {
            ServerCache.Clear();
        }

        public ServerResource TryGetValue(ulong key)
        {
            return ServerCache.TryGetValue(key, out ServerResource server) ? server : null;
        }

        public void TryAddValue(ulong discordId, ServerResource result)
        {
            if (ServerCache.ContainsKey(discordId))
            {
                ServerCache[discordId] = result;
            }
            ServerCache.TryAdd(discordId, result);
        }
    }
}
