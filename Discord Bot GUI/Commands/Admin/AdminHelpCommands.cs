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

namespace Discord_Bot.Commands.Admin
{
    public class AdminHelpCommands(
        IServerService serverService,
        Logging logger,
        Config config) : BaseCommand(logger, config, serverService)
    {
        [Command("help admin")]
        [Alias(["help a"])]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Embed complete list of commands in a text file")]
        public async Task Help()
        {
            try
            {
                Dictionary<string, string> commands = [];

                if (!File.Exists("Assets\\Commands\\Admin_Commands.txt"))
                {
                    await ReplyAsync("List of commands can't be found!");
                    return;
                }

                Embed[] embed = HelpAdminEmbedProcessor.CreateEmbed(config.Img);

                await ReplyAsync(embeds: embed);
            }
            catch (Exception ex)
            {
                logger.Error("AdminHelpCommands.cs Help", ex);
            }
        }

        [Command("feature")]
        [Alias(["features"])]
        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [RequireContext(ContextType.Guild)]
        [Summary("Embed complete list of features")]
        public async Task Features()
        {
            try
            {
                Dictionary<string, string> commands = [];

                if (!File.Exists("Assets\\Commands\\Features.txt"))
                {
                    await ReplyAsync("List of commands can't be found!");
                    return;
                }

                Embed[] embed = FeatureEmbedProcessor.CreateEmbed(config.Img);

                await ReplyAsync(embeds: embed);
            }
            catch (Exception ex)
            {
                logger.Error("AdminHelpCommands.cs Features", ex);
            }
        }
    }
}
