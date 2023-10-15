namespace Discord_Bot.Resources
{
    public class TwitchChannelResource
    {
        public ulong TwitchDiscordId { get; set; }
        public string TwitchId { get; set; }
        public string TwitchLink { get; set; }
        public ulong ServerDiscordId { get; set; }
        public ulong? NotificationRoleDiscordId { get; set; }
        public string NotificationRoleName { get; set; }
    }
}
