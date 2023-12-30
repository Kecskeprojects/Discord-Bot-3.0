using Discord.Commands;
using Discord;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord_Bot.Core.Config;
using Discord_Bot.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Commands;

namespace Discord_Bot.Commands
{
    public class GreetingCommands(IGreetingService greetingService, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IGreetingCommands
    {
        private readonly IGreetingService greetingService = greetingService;

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
                logger.Error("GreetingCommands.cs GreetingList", ex.ToString());
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
                logger.Error("GreetingCommands.cs GreetingAdd", ex.ToString());
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
                logger.Error("GreetingCommands.cs GreetingRemove", ex.ToString());
            }
        }
    }
}
