using Discord;
using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.Owner
{
    internal class OwnerHelpCommands(IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService)
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

                Embed[] embed = HelpOwnerEmbedProcessor.CreateEmbed(config.Img);

                await ReplyAsync(embeds: embed);
            }
            catch (Exception ex)
            {
                logger.Error("OwnerHelpCommands.cs Help", ex);
            }
        }
    }
}
