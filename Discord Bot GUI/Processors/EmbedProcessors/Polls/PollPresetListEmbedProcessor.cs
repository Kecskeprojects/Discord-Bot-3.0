using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors.Polls;

public class PollPresetListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<WeeklyPollOptionPresetResource> list)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle("Weekly Poll Presets:");

        foreach (WeeklyPollOptionPresetResource preset in list)
        {
            string body = $"{(preset.IsActive ? "Active" : "Inactive")} {(preset.IsSpecialPreset ? "Special" : "Standard")} option preset\nDescription: {preset.Description}";
            _ = builder.AddField(preset.Name, body);
        }

        _ = builder.WithColor(Color.DarkBlue);
        return [builder.Build()];
    }
}
