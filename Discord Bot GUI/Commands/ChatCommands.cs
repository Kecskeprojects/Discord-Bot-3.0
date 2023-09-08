using Discord;
using Discord.Commands;
using Discord_Bot.CommandsService;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.Commands;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Resources;
using Discord_Bot.Services.Models.Wotd;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class ChatCommands : CommandBase, IChatCommands
    {
        private readonly ICustomCommandService customCommandService;
        private readonly IWordOfTheDayService wordOfTheDayService;

        public ChatCommands(ICustomCommandService customCommandService, IWordOfTheDayService wordOfTheDayService, Logging logger, Config config) : base(logger, config)
        {
            this.customCommandService = customCommandService;
            this.wordOfTheDayService = wordOfTheDayService;
        }

        #region 8ball command
        [Command("8ball")]
        [Summary("8ball game, takes in the whole message, returns random 8ball answer from array")]
        public async Task Eightball([Remainder] string question)
        {
            try
            {
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
        #endregion

        #region Custom command list command
        [Command("custom list")]
        [RequireContext(ContextType.Guild)]
        [Alias(new string[] { "customlist", "customcommands" })]
        [Summary("Command to list out all the currently available commands")]
        public async Task CustomList()
        {
            try
            {
                List<CustomCommandResource> list = await customCommandService.GetServerCustomCommandListAsync(Context.Guild.Id);
                if (CollectionTools.IsNullOrEmpty(list))
                {
                    await ReplyAsync("There are no custom commands on this server!");
                    return;
                }

                EmbedBuilder builder = ChatService.BuildCustomListEmbed(list);

                await ReplyAsync("", false, builder.Build());
            }
            catch (Exception ex)
            {
                logger.Error("ChatCommands.cs CustomList", ex.ToString());
            }
        }
        #endregion

        #region Help command
        [Command("help")]
        [Summary("List out commands everybody has access to")]
        public async Task Help()
        {
            try
            {
                Dictionary<string, string> commands = new();

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
        #endregion

        #region Coin flip command
        [Command("coin flip")]
        [Alias(new string[] { "flip a coin", "flip coin", "flipcoin", "cf", "fc", "cofl", "flco", "coin", "flip", "coinflip", "50/50", "pick" })]
        [Summary("A 50/50 type decision maker")]
        public async Task CoinFlip([Remainder] string choice = "")
        {
            try
            {
                Random r = new();
                int chance = r.Next(1, 101);

                string[] choices = new string[] { "Heads", "Tails" };

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
        #endregion

        #region Word of the day command
        [Command("wotd")]
        [Alias(new string[] { "word of the day" })]
        [Summary("Learn a word a day command")]
        public async Task WotdFunction(string language = "korean")
        {
            try
            {
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
        #endregion
    }
}
