using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors.Polls;

public class PollPresetListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<WeeklyPollOptionPresetResource> list)
    {
        EmbedBuilder builder = new();
        builder.WithTitle("Weekly Poll Presets:");

        foreach (WeeklyPollOptionPresetResource preset in list)
        {
            string body = $"{(preset.IsActive ? "Active" : "Inactive")} {(preset.IsSpecialPreset ? "Special" : "Standard")} option preset\nDescription: {preset.Description}";
            builder.AddField(preset.Name, body);
        }

        builder.WithColor(Color.DarkBlue);
        return [builder.Build()];
    }
}
