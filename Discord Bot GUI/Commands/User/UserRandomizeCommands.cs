using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            _ = chance < 50
                ? await ReplyAsync("The coin landed on: " + choices[0].Trim())
                : chance > 51 ? await ReplyAsync("The coin landed on: " + choices[1].Trim()) : await ReplyAsync("The coin landed on it's edge");
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
    public async Task RainbowRandomizer([Name("attacker (a) or defender (d)")] string optionString, [Remainder][Name("channel name")] IChannel channel = null)
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            List<string> operatorList;
            if (optionString is "a" or "attacker")
            {
                operatorList = File.ReadAllLines("R6_Attackers.txt").ToList();
            }
            else if (optionString is "d" or "defender")
            {
                operatorList = File.ReadAllLines("R6_Defenders.txt").ToList();
            }
            else
            {
                _ = await ReplyAsync("Please specify either `attacker` or `defender`");
                return;
            }

            List<SocketVoiceChannel> voiceChannels = Context.Guild?.Channels?.Where(x => x is SocketVoiceChannel).Select(x => (SocketVoiceChannel) x).ToList();
            SocketVoiceChannel currentVoiceChannel = voiceChannels?.FirstOrDefault(x => x.ConnectedUsers.Any(y => y.Id == Context.User.Id));

            List<string> responseParts = [];
            if (DiscordTools.IsDM(Context) || currentVoiceChannel == null)
            {
                Random r_personal = new();

                string chosen_personal = operatorList[r_personal.Next(0, operatorList.Count)];

                responseParts.Add($"{Context.User.GlobalName ?? Context.User.Username}: {chosen_personal}");
            }
            else
            {
                Random r = new();
                foreach (SocketGuildUser user in currentVoiceChannel.ConnectedUsers)
                {
                    int index = r.Next(0, operatorList.Count);
                    string chosen = operatorList[index];
                    responseParts.Add($"{user.GlobalName ?? user.Username}: {chosen}");

                    operatorList .RemoveAt(index);
                }
            }

            if(channel is not null and SocketTextChannel textChannel)
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
}
