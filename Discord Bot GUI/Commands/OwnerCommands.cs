using Discord;
using Discord.Commands;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class OwnerCommands(IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService)
    {
        [Command("help owner")]
        [RequireOwner]
        [Summary("Embed complete list of commands in a text file")]
        public async Task Help()
        {
            try
            {
                Dictionary<string, string> commands = [];

                if (!File.Exists("Assets\\Commands\\Owner_Commands.txt"))
                {
                    await ReplyAsync("List of commands can't be found!");
                    return;
                }

                OwnerService.ReadCommandsFile(commands);
                EmbedBuilder builder = OwnerService.BuildHelpEmbed(commands, config.Img);

                await ReplyAsync("", false, builder.Build());
            }
            catch (Exception ex)
            {
                logger.Error("OwnerCommands.cs Help", ex);
            }
        }

        [Command("say")]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
        [Summary("Command for owner, the bot says in whatever channel you gave it what you told it to say")]
        public async Task Say(IMessageChannel channel, [Remainder] string text)
        {
            if (Context.Guild.TextChannels.Contains(channel))
            {
                await Context.Message.DeleteAsync();

                await channel.SendMessageAsync(text);
            }
        }
    }
}
