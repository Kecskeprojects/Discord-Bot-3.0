using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Tools;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class UserReminderListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<ReminderResource> list)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle("Your reminders:");

        int i = 1;
        foreach (ReminderResource reminder in list)
        {
            _ = builder.AddField($"#{i} {DiscordTools.UtcTimestamp(reminder.Date, TimestampTagStyles.ShortDateTime)}", reminder.Message);
            i++;
        }
        return [builder.Build()];
    }
}
