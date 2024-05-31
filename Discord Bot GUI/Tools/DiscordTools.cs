using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Tools;

public static class DiscordTools
{
    public static bool IsTypeOfChannel(ServerResource server, ChannelTypeEnum type, ulong channelId, bool allowLackOfType = true)
    {
        return server == null || (!server.SettingsChannels.TryGetValue(type, out List<ulong> value)
            ? allowLackOfType
            : value.Contains(channelId));
    }
}
