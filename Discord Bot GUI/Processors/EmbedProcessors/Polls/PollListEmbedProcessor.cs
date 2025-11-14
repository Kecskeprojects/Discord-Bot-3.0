using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors.Polls;

public class PollListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<WeeklyPollResource> list)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle("Server Weekly Polls:");

        foreach (WeeklyPollResource poll in list)
        {
            string body = $"{(poll.IsMultipleAnswer ? "Multi" : "Single")} answer poll with {poll.Options.Count} answers";
            _ = builder.AddField(poll.Name, body);
        }

        _ = builder.WithColor(Color.DarkBlue);
        return [builder.Build()];
    }
}
