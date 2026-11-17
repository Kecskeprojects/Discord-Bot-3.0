using DatabaseORM.Enums;

namespace DatabaseORM.Resources;

public class ServerResource
{
    public int ServerId { get; set; }

    public ulong DiscordId { get; set; }

    public ulong? RoleMessageDiscordId { get; set; }

    public ulong? NotificationRoleDiscordId { get; set; }

    public string? NotificationRoleName { get; set; }

    public ulong? MuteRoleDiscordId { get; set; }

    public string? MuteRoleName { get; set; }

    public List<TwitchChannelResource> TwitchChannels { get; set; } = [];

    public Dictionary<ChannelTypeEnum, List<ulong>> SettingsChannels { get; set; } = [];
}
