using Discord_Bot.Enums;

namespace Discord_Bot.Resources
{
    public class ServerSettingsChannelResource
    {
        public ServerSettingsChannelResource(string discordId, int type)
        {
            DiscordId = ulong.TryParse(discordId, out ulong res) ? res : 0;
            Type = (ChannelTypeEnum)type;
        }

        public ulong DiscordId { get; set; }
        public ChannelTypeEnum Type { get; set; }
    }
}
