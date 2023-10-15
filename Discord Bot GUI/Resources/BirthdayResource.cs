using System;

namespace Discord_Bot.Resources
{
    public class BirthdayResource
    {
        public int BirthdayId { get; set; }
        public ulong ServerDiscordId { get; set; }
        public ulong UserDiscordId { get; set; }
        public DateTime Date { get; set; }
    }
}
