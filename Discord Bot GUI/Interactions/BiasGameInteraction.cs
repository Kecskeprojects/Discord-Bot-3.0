using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions;

public class BiasGameInteraction(
    IIdolService idolService,
    BiasGameImageProcessor biasGameImageProcessor,
    Logging logger,
    Config config) : BaseInteraction(logger, config)
{
    private readonly IIdolService idolService = idolService;
    private readonly BiasGameImageProcessor biasGameImageProcessor = biasGameImageProcessor;

    [ComponentInteraction("BiasGame_Setup_Gender_*_*")]
    public async Task GenderChosen(GenderChoiceEnum choiceId, ulong userId)
    {
        try
        {
            if (Context.User.Id != userId || !Global.BiasGames.TryGetValue(Context.User.Id, out BiasGameData data))
            {
                await RespondAsync("You are not the owner of this interaction-", ephemeral: true);
                return;
            }

            await DeferAsync();

            data.SetGender(choiceId);

            logger.Log($"BiasGame Setup Gender Chosen: {choiceId}", LogOnly: true);
            MessageComponent component = BiasGameDebutEmbedProcessor.CreateComponent(data);

            await ModifyOriginalResponseAsync(x => x.Components = component);
        }
        catch (Exception ex)
        {
            logger.Error("BiasGameInteraction.cs GenderChoosen", ex);
            Global.BiasGames.TryRemove(Context.User.Id, out _);
            await FollowupAsync("Failure during setup!");
        }
    }

    [ComponentInteraction("BiasGame_Setup_Debut_*")]
    public async Task DebutChosen(ulong userId, string[] chosenYears)
    {
        try
        {
            if (Context.User.Id != userId || !Global.BiasGames.TryGetValue(Context.User.Id, out BiasGameData data))
            {
                await RespondAsync("You are not the owner of this interaction-", ephemeral: true);
                return;
            }

            data.SetDebut(chosenYears);

            logger.Log($"BiasGame Setup Debut Chosen: {string.Join(", ", chosenYears)}", LogOnly: true);

            SocketMessageComponent component = Context.Interaction as SocketMessageComponent;

            ComponentBuilder waitButton = new ComponentBuilder()
                .WithButton("Your game is being set up...", "BiasGame_Setup_Debut_Disabled", disabled: true, style: ButtonStyle.Secondary);

            await component.UpdateAsync(x => x.Components = waitButton.Build());

            await CreatePolaroids(data);

            int[] idolIds = data.Pairs[data.CurrentPair];

            Stream combined = biasGameImageProcessor.CombineImages(
                (MemoryStream) data.IdolWithImage[idolIds[0]].Stream,
                (MemoryStream) data.IdolWithImage[idolIds[1]].Stream);
            List<FileAttachment> files = [new FileAttachment(combined, "combined.png")];

            Embed[] embeds = BiasGameEmbedProcessor.CreateEmbed(
                data,
                GetCurrentUserAvatar(),
                GetCurrentUserNickname());

            MessageComponent components = BiasGameEmbedProcessor.CreateComponent(idolIds, Context.User.Id);

            await DeleteOriginalResponseAsync();

            //Followup will respond with the first embed
            IUserMessage message = await FollowupWithFilesAsync(files, embeds: embeds, components: components);
            data.MessageId = message.Id;
        }
        catch (Exception ex)
        {
            logger.Error("BiasGameInteraction.cs DebutChosen", ex);
            Global.BiasGames.TryRemove(Context.User.Id, out _);
            await RespondAsync("Failure during setup!");
        }
    }

    private async Task CreatePolaroids(BiasGameData data)
    {
        List<IdolGameResource> idols = await idolService.GetListForGameAsync(data.Gender, data.DebutYearStart, data.DebutYearEnd);

        if (idols.Count < 16)
        {
            await DeleteOriginalResponseAsync();

            await FollowupAsync("Not enough idols with your selected parameters!");
            Global.BiasGames.TryRemove(Context.User.Id, out _);
            return;
        }

        foreach (IdolGameResource idol in idols)
        {
            Stream stream = await biasGameImageProcessor.CreatePolaroid(idol);
            string fileName = $"{Guid.NewGuid()}_{idol.IdolId}.png";

            data.AddImage(idol.IdolId, stream, fileName);
        }

        data.CreatePairs();
    }
}
