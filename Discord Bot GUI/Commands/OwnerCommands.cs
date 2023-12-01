using Discord;
using Discord.Commands;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class OwnerCommands(IGreetingService greetingService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IOwnerCommands
    {
        private readonly IGreetingService greetingService = greetingService;

        [Command("help owner")]
        [RequireOwner]
        [Summary("Embed complete list of commands in a text file")]
        public async Task Help()
        {
            try
            {
                if (!File.Exists("Assets\\Commands\\All_Commands.txt"))
                {
                    await ReplyAsync("Command file missing!");
                    return;
                }

                await Context.Channel.SendFileAsync(Directory.GetCurrentDirectory() + "\\Assets\\Commands\\All_Commands.txt");
            }
            catch (Exception ex)
            {
                logger.Error("OwnerCommands.cs Help", ex.ToString());
            }
        }

        #region Greeting commands
        [Command("greeting list")]
        [RequireOwner]
        [Summary("Command for owner to list global greeting gifs")]
        public async Task GreetingList()
        {
            try
            {
                List<GreetingResource> greetings = await greetingService.GetAllGreetingAsync();
                if (!CollectionTools.IsNullOrEmpty(greetings))
                {
                    EmbedBuilder builder = new();
                    builder.WithTitle("Greetings:");

                    int i = 1;
                    foreach (GreetingResource greeting in greetings)
                    {
                        builder.AddField($"ID:{greeting.GreetingId}", greeting.Url);
                        i++;
                    }

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception ex)
            {
                logger.Error("OwnerCommands.cs GreetingList", ex.ToString());
            }
        }

        [Command("greeting add")]
        [RequireOwner]
        [Summary("Command for owner to add global greeting gifs")]
        public async Task GreetingAdd(string url)
        {
            try
            {
                DbProcessResultEnum result = await greetingService.AddGreetingAsync(url);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync("Greeting added!");
                }
                else
                {
                    await ReplyAsync("Greeting could not be added!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("OwnerCommands.cs GreetingAdd", ex.ToString());
            }
        }

        [Command("greeting remove")]
        [RequireOwner]
        [Summary("Command for owner to remove global greeting gifs")]
        public async Task GreetingRemove(int id)
        {
            try
            {
                DbProcessResultEnum result = await greetingService.RemoveGreetingAsync(id);
                if (result == DbProcessResultEnum.Success)
                {
                    await ReplyAsync($"Greeting with the ID {id} has been removed!");
                }
                else if (result == DbProcessResultEnum.NotFound)
                {
                    await ReplyAsync("Greeting doesn't exist with that ID or it is not yours!");
                }
                else
                {
                    await ReplyAsync("Greeting could not be removed!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("OwnerCommands.cs GreetingRemove", ex.ToString());
            }
        }
        #endregion

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
