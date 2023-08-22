using Discord_Bot.Enums;
using System.Collections.Generic;

namespace Discord_Bot.Resources
{
    public class ServerResource
    {
        public int ServerId { get; set; }
        public ulong DiscordId { get; set; }
        public Dictionary<ChannelTypeEnum, List<ulong>> SettingsChannels { get; set; }
        public List<TwitchChannelResource> TwitchChannels { get; set; }
        public AudioVariables AudioVariables { get; set; }
        public List<MusicRequest> MusicRequests { get; set; }
    }
}
