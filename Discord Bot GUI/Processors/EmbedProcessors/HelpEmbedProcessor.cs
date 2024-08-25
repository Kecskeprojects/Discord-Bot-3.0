using Discord;
using Discord.Commands;
using Discord_Bot.Enums;
using System.Collections.Generic;

namespace Discord_Bot.Processors.EmbedProcessors;
public class HelpEmbedProcessor
{
    public static Embed[] CreateEmbed(CommandLevelEnum commandLevel, List<ModuleInfo> modules, string imageUrl)
    {
        EmbedBuilder builder = new();
        builder.WithTitle($"{commandLevel.ToFriendlyString()} Commands");

        switch (commandLevel)
        {
            case CommandLevelEnum.Owner:
                builder.AddField("!help [o/own/owner]", "List out command categories for the owner");
                break;
            case CommandLevelEnum.Admin:
                builder.AddField("!help [a/adm/admin]", "List out command categories for admins");
                break;
            case CommandLevelEnum.User:
                builder.AddField("!help", "List out command categories for users");
                break;
        }

        foreach (ModuleInfo module in modules)
        {
            builder.AddField(module.Name, module.Summary);
        }

        builder.WithThumbnailUrl(imageUrl);
        builder.WithColor(Color.Orange);
        builder.WithCurrentTimestamp();
        return [builder.Build()];
    }

    public static MessageComponent CreateComponent(CommandLevelEnum commandLevel, List<ModuleInfo> modules)
    {

        SelectMenuBuilder selectMenu = new();
        selectMenu.WithCustomId($"HelpMenu_{commandLevel}");
        selectMenu.WithPlaceholder("Select a category to see details of commands...");

        modules.ForEach(y => selectMenu.AddOption(y.Name, y.Name, $"{y.Commands.Count} commands..."));

        ComponentBuilder components = new();
        components.WithSelectMenu(selectMenu);
        return components.Build();
    }
}
