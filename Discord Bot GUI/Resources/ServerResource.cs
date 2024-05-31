using Discord_Bot.Enums;
using System.Collections.Generic;

namespace Discord_Bot.Resources;

public class ServerResource
{
    public int ServerId { get; set; }

    public ulong DiscordId { get; set; }

    public ulong? RoleMessageDiscordId { get; set; }

    public ulong? NotificationRoleDiscordId { get; set; }

    public string NotificationRoleName { get; set; }

    public List<TwitchChannelResource> TwitchChannels { get; set; }

    public Dictionary<ChannelTypeEnum, List<ulong>> SettingsChannels { get; set; }
}
