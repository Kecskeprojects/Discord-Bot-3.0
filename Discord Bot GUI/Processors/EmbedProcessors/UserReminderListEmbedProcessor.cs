using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public class UserReminderListEmbedProcessor
    {
        public static Embed[] CreateEmbed(List<ReminderResource> list)
        {
            EmbedBuilder builder = new();
            builder.WithTitle("Your reminders:");

            int i = 1;
            foreach (ReminderResource reminder in list)
            {
                builder.AddField($"#{i} {TimestampTag.FromDateTime(reminder.Date, TimestampTagStyles.ShortDateTime)}", reminder.Message);
                i++;
            }
            return [builder.Build()];
        }
    }
}
