using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.Core;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands.User;

[Name("Randomizer")]
[Remarks("User")]
[Summary("Random result generator")]
public class UserRandomizeCommands(
    IServerService serverService,
    IBotLogger logger,
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

            //50 and 51, 2 in 100, are the coin landing on its edge
            string result = chance switch
            {
                < 50 => "The coin landed on: " + choices[0].Trim(),
                > 51 => "The coin landed on: " + choices[1].Trim(),
                _ => "The coin landed on it's edge"
            };
            _ = await ReplyAsync(result);
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

    [Command("r6 randomizer")]
    [Alias(["r6 rand", "r6 r", "rainbow randomizer", "rainbow rand", "rainbow r"])]
    [Summary("Randomized operators (defender and attacker) for all users in the user's voice chat")]
    public async Task RainbowRandomizer([Name("attacker (a) or defender (d)")] string optionString, [Remainder][Name("channel name")] IChannel? channel = null)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            if (optionString is not "a" and not "attacker" and not "d" and not "defender")
            {
                _ = await ReplyAsync("Please specify either `attacker` or `defender`");
                return;
            }

            List<string> operatorList = RainbowRandomizerTools.GetOperatorList(optionString);
            List<SocketUser> userList = RainbowRandomizerTools.GetUserList(Context);

            List<string> responseParts = RainbowRandomizerTools.GetResponseForRound(operatorList, userList);

            if (channel is not null and SocketTextChannel textChannel)
            {
                _ = await textChannel.SendMessageAsync(string.Join("\n", responseParts));
                return;
            }
            _ = await ReplyAsync(string.Join("\n", responseParts));
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs RainbowRandomizer", ex);
        }
    }

    [Command("r6 full randomizer")]
    [Alias(["r6 full rand", "r6 f r", "rainbow full randomizer", "rainbow full rand", "rainbow f r"])]
    [Summary("Randomized operators (defender and attacker) for all users in the user's voice chat")]
    public async Task RainbowFullRandomizer([Name("attacker (a) or defender (d)")] string optionString, [Remainder][Name("channel name")] IChannel? channel = null)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            if (optionString is not "a" and not "attacker" and not "d" and not "defender")
            {
                _ = await ReplyAsync("Please specify either `attacker` or `defender`");
                return;
            }

            List<string> operatorList = RainbowRandomizerTools.GetOperatorList(optionString);
            List<string> otherOperatorList = RainbowRandomizerTools.GetOperatorList(optionString is "a" or "attacker" ? "defender" : "attacker");
            List<SocketUser> userList = RainbowRandomizerTools.GetUserList(Context);

            List<string> finalResponseParts = [];
            string label = optionString is "a" or "attacker" ? "Attacker" : "Defender";
            string otherLabel = optionString is "a" or "attacker" ? "Defender" : "Attacker";
            for (int i = 0; i < 6; i++)
            {
                string currentLabel = i / 3 < 1 ? label : otherLabel;
                List<string> currentOperatorList = i / 3 < 1 ? operatorList : otherOperatorList;

                List<string> responseParts = RainbowRandomizerTools.GetResponseForRound(currentOperatorList, userList);
                finalResponseParts.Add($"\n{currentLabel} Round {i + 1}:");
                finalResponseParts.AddRange(responseParts);
            }

            if (channel is not null and SocketTextChannel textChannel)
            {
                _ = await textChannel.SendMessageAsync(string.Join("\n", finalResponseParts));
                return;
            }
            _ = await ReplyAsync(string.Join("\n", finalResponseParts));
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs RainbowFullRandomizer", ex);
        }
    }

    [Command("r6 overtime randomizer")]
    [Alias(["r6 over rand", "r6 o r", "rainbow overtime randomizer", "rainbow over rand", "rainbow o r"])]
    [Summary("Randomized operators (defender and attacker) for all users in the user's voice chat")]
    public async Task RainbowOvertimeRandomizer([Name("attacker (a) or defender (d)")] string optionString, [Remainder][Name("channel name")] IChannel? channel = null)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            if (optionString is not "a" and not "attacker" and not "d" and not "defender")
            {
                _ = await ReplyAsync("Please specify either `attacker` or `defender`");
                return;
            }
            List<string> operatorList = RainbowRandomizerTools.GetOperatorList(optionString);
            List<string> otherOperatorList = RainbowRandomizerTools.GetOperatorList(optionString is "a" or "attacker" ? "defender" : "attacker");
            List<SocketUser> userList = RainbowRandomizerTools.GetUserList(Context);

            List<string> finalResponseParts = [];
            string label = optionString is "a" or "attacker" ? "Attacker" : "Defender";
            string otherLabel = optionString is "a" or "attacker" ? "Defender" : "Attacker";
            for (int i = 0; i < 3; i++)
            {
                string currentLabel = i % 2 != 1 ? label : otherLabel;
                List<string> currentOperatorList = i % 2 != 1 ? operatorList : otherOperatorList;

                List<string> responseParts = RainbowRandomizerTools.GetResponseForRound(currentOperatorList, userList);
                finalResponseParts.Add($"\n{currentLabel} Round {i + 1}:");
                finalResponseParts.AddRange(responseParts);
            }

            if (channel is not null and SocketTextChannel textChannel)
            {
                _ = await textChannel.SendMessageAsync(string.Join("\n", finalResponseParts));
                return;
            }
            _ = await ReplyAsync(string.Join("\n", finalResponseParts));
        }
        catch (Exception ex)
        {
            logger.Error("UserRandomizeCommands.cs RainbowOvertimeRandomizer", ex);
        }
    }
}
