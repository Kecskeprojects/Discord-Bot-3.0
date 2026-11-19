namespace DatabaseORM.Resources;

public class TwitchChannelResource
{
    public string TwitchId { get; set; } = "";

    public string TwitchLink { get; set; } = "";

    public ulong ServerDiscordId { get; set; }

    public ulong? NotificationRoleDiscordId { get; set; }

    public string? NotificationRoleName { get; set; }
}
