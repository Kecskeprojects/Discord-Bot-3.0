using Discord_Bot.Resources;

namespace Discord_Bot.Core.Caching
{
    public class Cache
    {
        private SizedDictionary<ulong, ServerResource> ServerCache { get; } = new(10);

        public void RemoveCachedEntityManually(ulong key)
        {
            if (ServerCache.ContainsKey(key))
            {
                ServerCache.Remove(key);
            }
        }

        public void ClearCachedEntityManually() => ServerCache.Clear();
        public ServerResource TryGetValue(ulong key)
        {
            if(ServerCache.TryGetValue(key, out ServerResource server))
            {
                return server;
            }
            return null;
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
