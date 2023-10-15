using System;

namespace Discord_Bot.Resources
{
    public class ReminderResource
    {
        public int ReminderId { get; set; }
        public ulong UserDiscordId { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}
