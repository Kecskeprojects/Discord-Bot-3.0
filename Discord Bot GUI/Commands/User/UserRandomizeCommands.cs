using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Randomizer")]
[Remarks("User")]
[Summary("Random result generator")]
public class UserRandomizeCommands(
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    [Command("8ball")]
    [Summary("8ball game, takes in a question, returns a cryptic response")]
    public async Task Eightball([Remainder] string question)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(question))
            {
                _ = await ReplyAsync("Ask me about something!");
                return;
            }

            _ = await ReplyAsync(Constant.Answers8ball[new Random().Next(0, Constant.Answers8ball.Length)]);
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs Eightball", ex);
        }
    }

    [Command("coin flip")]
    [Alias(["flip a coin", "flip coin", "flipcoin", "cf", "fc", "cofl", "flco", "coin", "flip", "coinflip", "50/50", "pick"])]
    [Summary("A 50/50 type decision maker")]
    public async Task CoinFlip([Name("choice 1 or choice 2")][Remainder] string choice = "")
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            Random r = new();
            int chance = r.Next(0, 100);

            string[] choices = ["Heads", "Tails"];

            //If choice options are given, we switch out the original strings
            string[] paramArray = GetParametersBySplit(choice, " or ");
            if (paramArray.Length > 1)
            {
                choices = paramArray;
            }

            if (chance < 50)
            {
                _ = await ReplyAsync("The coin landed on: " + choices[0].Trim());
            }
            else
            {
                _ = chance > 51 ? await ReplyAsync("The coin landed on: " + choices[1].Trim()) : await ReplyAsync("The coin landed on it's edge");
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs CoinFlip", ex);
        }
    }

    [Command("decide")]
    [Summary("Random choice from multiple options")]
    public async Task Decide([Name("opt1,opt2...")][Remainder] string optionString)
    {
        try
        {
            string[] options = GetParametersBySplit(optionString, ',', false);

            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            Random r = new();

            string chosen = options[r.Next(0, options.Length)];

            _ = await ReplyAsync(chosen);
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs Decide", ex);
        }
    }
}
