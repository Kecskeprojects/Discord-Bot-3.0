using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Discord_Bot.Processors.EmbedProcessors;
public class HelpDetailEmbedProcessor
{
    public static Embed[] CreateEmbed(CommandLevelEnum commandLevel, string category, List<CommandInfo> commands, string imageUrl)
    {
        EmbedBuilder builder = new();
        builder.WithTitle($"{commandLevel.ToFriendlyString()} {category} Commands:");

        foreach (CommandInfo command in commands)
        {
            string parameters = string.Join(" ", command.Parameters.Select(x => $"[{x.Name}]"));
            foreach (string divider in Constant.SpecialCommandParameterDividers)
            {
                parameters = parameters.Replace(divider, $"]{divider}[");
            }

            bool canBeUsedInDM = !command.Preconditions.Any(x => x is RequireContextAttribute contextAttribute && contextAttribute.Contexts == ContextType.Guild);

            string aliases = string.Join(", ", command.Aliases.SkipWhile(x => x == command.Name));

            builder.AddField(
                $"!{command.Name} {parameters}",
                $"{(canBeUsedInDM ? "(DM) " : "")}{command.Summary}{(!string.IsNullOrEmpty(aliases) ? $"\nAliases: {aliases}" : "")}"
                );
        }

        builder.WithThumbnailUrl(imageUrl);
        builder.WithColor(Color.Orange);
        builder.WithCurrentTimestamp();
        return [builder.Build()];
    }
}
