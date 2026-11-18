using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Interfaces;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;

namespace Discord_Bot.Commands.User;

[Name("Bias Game")]
[Remarks("User")]
[Summary("Elimination tournament to decide your favorite idol")]
public class UserBiasGameCommands(
    IUserService userService,
    BiasGameStore biasGames,
    IServerService serverService,
    IBotLogger logger,
    IConfig config) : BaseCommand(logger, config, serverService)
{
    private readonly IUserService userService = userService;
    private readonly BiasGameStore biasGames = biasGames;

    [Command("bias game")]
    [Alias(["bg", "biasgame"])]
    [Summary("Start a new game for you")]
    public async Task BiasGame()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            if (biasGames.TryStart(Context.User.Id) == null)
            {
                _ = await ReplyAsync("You already have a game going!");
                return;
            }

            Embed[] embed = BiasGameGenderEmbedProcessor.CreateEmbed(GetCurrentUserNickname(), GetCurrentUserAvatar());
            MessageComponent components = BiasGameGenderEmbedProcessor.CreateComponent(Context.User.Id);

            _ = await ReplyAsync(components: components, embeds: embed);
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasGameCommands.cs BiasGame", ex);
        }
    }

    [Command("bias game stop")]
    [Alias(["bgs", "biasgamestop"])]
    [Summary("Stop your current game, if any are ongoing")]
    public async Task BiasGameStop()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            Random r = new();
            if (r.Next(0, 100) == 0)
            {
                _ = await ReplyAsync(Constant.BiasGameStopMessages[r.Next(0, Constant.BiasGameStopMessages.Length)]);
                return;
            }

            using (BiasGameData? data = biasGames.Take(Context.User.Id))
            {
                if (data != null)
                {
                    _ = await ReplyAsync("Game stopped!");
                    await Context.Channel.DeleteMessageAsync(data.MessageId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasGameCommands.cs BiasGameStop", ex);
        }
    }

    [Command("bias stats")]
    [Alias(["bs", "biasstats", "biastats", "bias stat", "biasstat", "biastat"])]
    [Summary("Show your game statistics")]
    public async Task BiasStats()
    {
        try
        {
            if (!await IsCommandAllowedAsync(ChannelTypeEnum.CommandText, canBeDM: true))
            {
                return;
            }

            UserBiasGameStatResource? stats = await userService.GetTopIdolsAsync(Context.User.Id, GenderEnum.NotSpecified);

            if (stats == null || stats.BiasGameCount == 0 || stats.Stats.Count == 0)
            {
                _ = await ReplyAsync("You have not played any games!");
                return;
            }

            Embed[] embed = BiasGameStatEmbedProcessor.CreateEmbed(GetCurrentUserNickname(), GenderEnum.NotSpecified, stats);

            MessageComponent component = BiasGameStatEmbedProcessor.CreateComponent(GenderEnum.NotSpecified, Context.User.Id);

            _ = await ReplyAsync(embeds: embed, components: component);
        }
        catch (Exception ex)
        {
            logger.Error("UserBiasGameCommands.cs BiasStats", ex);
        }
    }
}
