using Discord;
using Discord.Commands;
using Discord_Bot.Enums;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors.Help;

public class HelpEmbedProcessor
{
    public static Embed[] CreateEmbed(CommandLevelEnum commandLevel, List<ModuleInfo> modules, string imageUrl)
    {
        EmbedBuilder builder = new();
        _ = builder.WithTitle($"{commandLevel} Commands");

        switch (commandLevel)
        {
            case CommandLevelEnum.Owner:
                _ = builder.AddField("!help [o/own/owner]", "List out command categories for the owner");
                break;
            case CommandLevelEnum.Admin:
                _ = builder.AddField("!help [a/adm/admin]", "List out command categories for admins");
                break;
            case CommandLevelEnum.User:
                _ = builder.AddField("!help", "List out command categories for users");
                break;
        }

        foreach (ModuleInfo module in modules)
        {
            _ = builder.AddField(module.Name, module.Summary);
        }

        _ = builder.WithThumbnailUrl(imageUrl);
        _ = builder.WithColor(Color.Orange);
        _ = builder.WithCurrentTimestamp();
        return [builder.Build()];
    }

    public static MessageComponent CreateComponent(CommandLevelEnum commandLevel, List<ModuleInfo> modules)
    {

        SelectMenuBuilder selectMenu = new();
        _ = selectMenu.WithCustomId($"HelpMenu_{commandLevel}");
        _ = selectMenu.WithPlaceholder("Select a category to see details of commands...");

        modules.ForEach(y => selectMenu.AddOption(y.Name, y.Name, $"{y.Commands.Count} commands..."));

        ComponentBuilder components = new();
        _ = components.WithSelectMenu(selectMenu);
        return components.Build();
    }
}
