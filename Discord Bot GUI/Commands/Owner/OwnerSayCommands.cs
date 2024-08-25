using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner;

//Todo: Change where role, user and channel names or ids are used as parameters to this implementation in the !say command
[Name("Say")]
[Remarks("Owner")]
[Summary("Send messages as the bot, the channel must be in the same server as the command")]
public class OwnerSayCommands(
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    [Command("say")]
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    [Summary("Send messages as the bot")]
    public async Task Say([Name("channel name")] IMessageChannel channel, [Remainder] string message)
    {
        try
        {
            if (Context.Guild.TextChannels.Contains(channel))
            {
                await Context.Message.DeleteAsync();

                await channel.SendMessageAsync(message);
            }
        }
        catch (Exception ex)
        {
            logger.Error("OwnerSayCommands.cs Say", ex);
        }
    }
}
