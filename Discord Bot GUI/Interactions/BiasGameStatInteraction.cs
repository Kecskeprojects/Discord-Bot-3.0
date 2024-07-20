using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions;

public class BiasGameStatInteraction(IUserService userService, BotLogger logger, Config config) : BaseInteraction(logger, config)
{
    private readonly IUserService userService = userService;

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

            logger.Log($"Bias Stat Gender Chosen: {chosenGender.ToFriendlyString()}", LogOnly: true);

            UserBiasGameStatResource stats = await userService.GetTopIdolsAsync(Context.User.Id, chosenGender);

            if (stats == null || stats.BiasGameCount == 0 || stats.Stats.Count == 0)
            {
                await FollowupAsync("Not enough choices made for this gender!", ephemeral: true);
                return;
            }

            Embed[] embed = BiasGameStatEmbedProcessor.CreateEmbed(GetCurrentUserNickname(), chosenGender, stats);

            MessageComponent component = BiasGameStatEmbedProcessor.CreateComponent(chosenGender, Context.User.Id);

            SocketMessageComponent message = Context.Interaction as SocketMessageComponent;
            await ModifyOriginalResponseAsync(x =>
            {
                x.Embeds = embed;
                x.Components = component;
            });
        }
        catch (Exception ex)
        {
            logger.Error("BiasGameInteraction.cs GenderChoosen", ex);
            Global.BiasGames.TryRemove(Context.User.Id, out _);
            await FollowupAsync("Failure during stat collection!");
        }
    }
}
