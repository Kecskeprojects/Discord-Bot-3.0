using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Interactions;
using Discord_Bot.Core;
using Discord_Bot.Interfaces;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;

namespace Discord_Bot.Interactions;

public class BiasGameStatInteraction(
    IUserService userService,
    BiasGameStore biasGames,
    IServerService serverService,
    IBotLogger logger,
    IConfig config) : BaseInteraction(serverService, logger, config)
{
    private readonly IUserService userService = userService;
    private readonly BiasGameStore biasGames = biasGames;

    [ComponentInteraction("BiasStats_Gender_*_*_*")]
    public async Task GenderChosen(GenderEnum chosenGender, GenderEnum currentGender, ulong userId)
    {
        try
        {
            if (userId != Context.User.Id)
            {
                await RespondAsync("You are not the owner of this interaction-", ephemeral: true);
                return;
            }

            await DeferAsync();

            if (chosenGender == currentGender)
            {
                return;
            }

            logger.Log($"Bias Stat Gender Chosen: {chosenGender.ToDatabaseFriendlyString()}", LogOnly: true);

            UserBiasGameStatResource? stats = await userService.GetTopIdolsAsync(Context.User.Id, chosenGender);

            if (stats == null || stats.BiasGameCount == 0 || stats.Stats.Count == 0)
            {
                _ = await FollowupAsync("Not enough choices made for this gender!", ephemeral: true);
                return;
            }

            Embed[] embed = BiasGameStatEmbedProcessor.CreateEmbed(GetCurrentUserNickname(), chosenGender, stats);

            MessageComponent component = BiasGameStatEmbedProcessor.CreateComponent(chosenGender, Context.User.Id);

            _ = await ModifyOriginalResponseAsync(x =>
            {
                x.Embeds = embed;
                x.Components = component;
            });
        }
        catch (Exception ex)
        {
            logger.Error("BiasGameStatInteraction.cs GenderChosen", ex);
            biasGames.Remove(Context.User.Id);
            await RespondOrFollowupAsync("Failure during stat collection!");
        }
    }
}
