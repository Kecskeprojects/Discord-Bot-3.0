using Discord_Bot.Resources;

namespace Discord_Bot.Core.Caching
{
    public class Cache
    {
        private SizedDictionary<ulong, ServerResource> _ServerCache;
        public SizedDictionary<ulong, ServerResource> ServerCache
        {
            get
            {
                _ServerCache ??= new(10);
                return _ServerCache;
            }
        }

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
    }
}
