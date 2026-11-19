using DatabaseORM.Caching;
using DatabaseORM.Resources;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Common.Tools;

//The test server in a real ServerCache, for checking that a settings change drops it
internal static class CachedServer
{
    //What IServerService.GetByDiscordIdAsync would have cached
    public static ServerResource Put(ServerCache cache)
    {
        ServerResource server = new() { ServerId = RowIds.Server, DiscordId = DiscordIds.Server };
        cache.TryAddValue(DiscordIds.Server, server);
        return server;
    }

    public static bool IsIn(ServerCache cache)
    {
        return cache.TryGetValue(DiscordIds.Server) != null;
    }
}
