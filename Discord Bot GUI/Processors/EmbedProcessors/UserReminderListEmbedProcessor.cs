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

        //Discord allows 25 fields in an embed; the list is by date, so these are the soonest, and !remind remove still takes any position
        int i = 1;
        foreach (ReminderResource reminder in list.Take(EmbedBuilder.MaxFieldCount))
        {
            _ = builder.AddField($"#{i} {DiscordTools.UtcTimestamp(reminder.Date, TimestampTagStyles.ShortDateTime)}", reminder.Message);
            i++;
        }

        if (list.Count > EmbedBuilder.MaxFieldCount)
        {
            _ = builder.WithFooter($"{list.Count - EmbedBuilder.MaxFieldCount} more not shown");
        }
        return [builder.Build()];
    }
}
