using System.Collections.Generic;

namespace Discord_Bot.Core.Caching
{
    public class Caching
    {
        Dictionary<CacheType, SizedDictionary<ulong, object>> _Cache;
        public Dictionary<CacheType, SizedDictionary<ulong, object>> Cache
        {
            get
            {
                _Cache ??= new()
                {
                    { CacheType.Server, new SizedDictionary<ulong, object>(3) }
                };
                return _Cache;
            }
        }

        public void RemoveCachedEntityManually(CacheType cacheType, ulong key)
        {
            if (Cache.ContainsKey(cacheType))
            {
                Cache[cacheType].Remove(key);
            }
        }
    }
}
