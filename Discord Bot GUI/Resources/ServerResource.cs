using System.Collections.Generic;

namespace Discord_Bot.Resources
{
    public class ServerResource
    {
        public ulong ServerId { get; set; }
        public ulong DiscordId { get; set; }
        //public ulong RoleChannel { get; set; }
        //public ulong[] MusicChannels { get; set; }
        public List<ServerSettingsChannelResource> SettingsChannels { get; set; }
        //public List<TwitchChannelResource> TwitchChannels { get; set; }
    }
}
