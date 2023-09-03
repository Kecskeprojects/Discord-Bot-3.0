using Discord_Bot.Resources;

namespace Discord_Bot.Core.Caching
{
    public class Cache
    {
        public SizedDictionary<ulong, ServerResource> ServerCache { get; } = new(10);

        public void RemoveCachedEntityManually(ulong key)
        {
            if (ServerCache.ContainsKey(key))
            {
                ServerCache.Remove(key);
            }
        }

        public void ClearCachedEntityManually() => ServerCache.Clear();
    }
}
