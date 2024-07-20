using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner;
public class OwnerSayCommands(
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    [Command("say")]
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    [Summary("Command for owner, the bot says in whatever channel you gave it what you told it to say")]
    public async Task Say(IMessageChannel channel, [Remainder] string text)
    {
        try
        {
            if (Context.Guild.TextChannels.Contains(channel))
            {
                await Context.Message.DeleteAsync();

                await channel.SendMessageAsync(text);
            }
        }
        catch (Exception ex)
        {
            logger.Error("OwnerSayCommands.cs Say", ex);
        }
    }
}
