using Discord.Commands;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

public class UserRandomizeCommands(
    IServerService serverService,
    BotLogger logger,
    Config config) : BaseCommand(logger, config, serverService)
{
    [Command("8ball")]
    [Summary("8ball game, takes in the whole message, returns random 8ball answer from array")]
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
                await ReplyAsync("Ask me about something!");
                return;
            }

            await ReplyAsync(StaticLists.Answers8ball[new Random().Next(0, StaticLists.Answers8ball.Length)]);
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs Eightball", ex);
        }
    }

    [Command("coin flip")]
    [Alias(["flip a coin", "flip coin", "flipcoin", "cf", "fc", "cofl", "flco", "coin", "flip", "coinflip", "50/50", "pick"])]
    [Summary("A 50/50 type decision maker")]
    public async Task CoinFlip([Remainder] string choice = "")
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
            logger.Error("UserRandomizeCommands.cs CoinFlip", ex);
        }
    }

    [Command("decide")]
    [Summary("Random from multiple options")]
    public async Task Decide([Remainder] string optionString)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            Random r = new();

            string[] options = optionString.Split(",", options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string chosen = options[r.Next(0, options.Length)];

            await ReplyAsync(chosen);
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs Decide", ex);
        }
    }
}
