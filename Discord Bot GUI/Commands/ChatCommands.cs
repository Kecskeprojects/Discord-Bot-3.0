using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Wotd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class ChatCommands(IWordOfTheDayService wordOfTheDayService, IPictureHandler pictureHandler, IServerService serverService, Logging logger, Config config) : BaseCommand(logger, config, serverService), IChatCommands
    {
        private readonly IWordOfTheDayService wordOfTheDayService = wordOfTheDayService;
        private readonly IPictureHandler pictureHandler = pictureHandler;

        [Command("help")]
        [Summary("List out commands everybody has access to")]
        public async Task Help()
        {
            try
            {
                if (Context.Channel.GetChannelType() != Discord.ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                Dictionary<string, string> commands = [];

                if (!File.Exists("Assets\\Commands\\Commands.txt"))
                {
                    await ReplyAsync("List of commands can't be found!");
                    return;
                }

                ChatService.ReadCommandsFile(commands);
                EmbedBuilder builder = ChatService.BuildHelpEmbed(commands, config.Img);

                await ReplyAsync("", false, builder.Build());
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs Help", ex.ToString());
            }
        }

        [Command("8ball")]
        [Summary("8ball game, takes in the whole message, returns random 8ball answer from array")]
        public async Task Eightball([Remainder] string question)
        {
            try
            {
                if (Context.Channel.GetChannelType() != Discord.ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(question))
                {
                    await ReplyAsync("Ask me about something!");
                    return;
                }

                await ReplyAsync(StaticLists.Answers8ball[new Random().Next(0, StaticLists.Answers8ball.Length)]);
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs Eightball", ex.ToString());
            }
        }

        [Command("coin flip")]
        [Alias(["flip a coin", "flip coin", "flipcoin", "cf", "fc", "cofl", "flco", "coin", "flip", "coinflip", "50/50", "pick"])]
        [Summary("A 50/50 type decision maker")]
        public async Task CoinFlip([Remainder] string choice = "")
        {
            try
            {
                if (Context.Channel.GetChannelType() != Discord.ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                Random r = new();
                int chance = r.Next(1, 101);

                string[] choices = ["Heads", "Tails"];

                //If choice options are given, we switch out the original strings
                if (choice != "" && choice.Contains(" or "))
                {
                    choices = choice.Split(" or ");
                }

                if (chance < 50)
                {
                    await ReplyAsync("The coin landed on: " + choices[0].Trim());
                }
                else if (chance > 51)
                {
                    await ReplyAsync("The coin landed on: " + choices[1].Trim());
                }
                else
                {
                    await ReplyAsync("The coin landed on it's edge");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs CoinFlip", ex.ToString());
            }
        }

        [Command("wotd")]
        [Alias(["word of the day"])]
        [Summary("Learn a word a day command")]
        public async Task WotdFunction(string language = "korean")
        {
            try
            {
                if (Context.Channel.GetChannelType() != Discord.ChannelType.DM)
                {
                    ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                    if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                    {
                        return;
                    }
                }

                WotdBase result = await wordOfTheDayService.GetDataAsync(language);

                if (result != null)
                {
                    EmbedBuilder embed = ChatService.BuildWotdEmbed(result);

                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    await ReplyAsync("Language is not supported, here is the list of languages:\n" + string.Join(", ", StaticLists.WotdLanguages.Keys));
                }
            }
            catch (HttpRequestException)
            {
                await ReplyAsync("Site is currently unavailable, try again in a little bit");
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs WotDFunction", ex.ToString());
            }
        }

        [Command("bonk")]
        [RequireContext(ContextType.Guild)]
        [Alias(["hornyjail, hit me please"])]
        [Summary("Inserts a gif with the user's local profile picture")]
        public async Task Bonk([Remainder] string parameters = "")
        {
            try
            {
                List<string> paramParts = [.. parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
                string userName = "";
                int frameDelay = 10;
                if (paramParts.Count > 0)
                {
                    if (int.TryParse(paramParts[^1], out frameDelay))
                    {
                        if (frameDelay < 1 || frameDelay > 1000)
                        {
                            await ReplyAsync("Invalid frame delay length. (1-1000)");
                            return;
                        }
                    }
                    else
                    {
                        frameDelay = 10;
                    }
                    paramParts.Remove(frameDelay.ToString());
                    userName = string.Join(" ", paramParts);
                }

                ServerResource server = await serverService.GetByDiscordIdAsync(Context.Guild.Id);
                if (!Global.IsTypeOfChannel(server, ChannelTypeEnum.CommandText, Context.Channel.Id))
                {
                    return;
                }

                string url = "";
                if (string.IsNullOrEmpty(userName))
                {
                    url = Context.User.GetDisplayAvatarUrl(ImageFormat.Png, 512);
                    userName = Global.GetNickName(Context.Channel, Context.User);
                }
                else
                {
                    await Context.Guild.DownloadUsersAsync();

                    if (ulong.TryParse(userName, out ulong userId))
                    {
                        SocketGuildUser user = Context.Guild.GetUser(userId);
                        url = user.GetDisplayAvatarUrl(ImageFormat.Png, 512);
                    }
                    else
                    {
                        IReadOnlyCollection<RestGuildUser> users = await Context.Guild.SearchUsersAsync(userName, 1);
                        if (users.Count > 0)
                        {
                            RestGuildUser user = users.First();
                            url = user.GetDisplayAvatarUrl(ImageFormat.Png, 512);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    Stream stream = await Global.GetStream(url);

                    MemoryStream gifStream = pictureHandler.CreateBonkImage(stream, frameDelay);

                    await Context.Channel.SendFileAsync(gifStream, $"bonk_{userName}.gif");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs Bonk", ex.ToString());
            }
        }
    }
}
