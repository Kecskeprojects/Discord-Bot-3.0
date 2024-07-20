using Discord;
using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors;

public static class CustomCommandListEmbedProcessor
{
    public static Embed[] CreateEmbed(List<CustomCommandResource> list)
    {
        EmbedBuilder builder = new();
        builder.WithTitle("Custom commands:");
        string commands = "";

        foreach (CustomCommandResource command in list)
        {
            if (commands == "")
            {
                commands += "!" + command.Command;
            }
            else
            {
                commands += " , !" + command.Command;
            }
        }
        builder.WithDescription(commands);
        builder.WithColor(Color.Teal);
        return [builder.Build()];
    }
}
