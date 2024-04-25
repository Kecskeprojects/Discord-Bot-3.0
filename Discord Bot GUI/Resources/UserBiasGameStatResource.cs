using System.Collections.Generic;

namespace Discord_Bot.Resources
{
    public class UserBiasGameStatResource
    {
        public int UserId { get; set; }

        public ulong DiscordId { get; set; }

        public int BiasGameCount { get; set; }

        public List<UserIdolStatisticResource> Stats { get; set; }
    }
}
