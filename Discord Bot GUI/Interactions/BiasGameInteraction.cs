using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Interactions;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Interfaces;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Processors.ImageProcessors;
using System.IO;

namespace Discord_Bot.Interactions;

public class BiasGameInteraction(
    IIdolService idolService,
    BiasGameImageProcessor biasGameImageProcessor,
    BiasGameStore biasGames,
    TimeProvider timeProvider,
    IServerService serverService,
    IBotLogger logger,
    IConfig config) : BaseInteraction(serverService, logger, config)
{
    private readonly IIdolService idolService = idolService;
    private readonly BiasGameImageProcessor biasGameImageProcessor = biasGameImageProcessor;
    private readonly BiasGameStore biasGames = biasGames;
    private readonly TimeProvider timeProvider = timeProvider;

    [ComponentInteraction("BiasGame_Setup_Gender_*_*")]
    public async Task GenderChosen(GenderEnum chosenGender, ulong userId)
    {
        try
        {
            if (Context.User.Id != userId || !biasGames.TryGet(Context.User.Id, out BiasGameData? data))
            {
                await RespondAsync("You are not the owner of this interaction.", ephemeral: true);
                return;
            }

            //A click must always be acknowledged within 3 seconds, otherwise Discord shows "This interaction failed"
            if (data.IsProcessing)
            {
                await DeferAsync();
                return;
            }

            data.IsProcessing = true;

            await DeferAsync();

            data.Gender = chosenGender;

            logger.Log($"BiasGame Setup Gender Chosen: {chosenGender.ToDatabaseFriendlyString()}", LogOnly: true);
            MessageComponent component = BiasGameDebutEmbedProcessor.CreateComponent(data, timeProvider.GetUtcNow().Year);

            _ = await ModifyOriginalResponseAsync(x => x.Components = component);
            data.IsProcessing = false;
        }
        catch (Exception ex)
        {
            logger.Error("BiasGameInteraction.cs GenderChosen", ex);
            biasGames.Remove(Context.User.Id);
            await RespondOrFollowupAsync("Failure during setup!");
        }
    }

    [ComponentInteraction("BiasGame_Setup_Debut_*")]
    public async Task DebutChosen(ulong userId, string[] chosenYears)
    {
        try
        {
            if (Context.User.Id != userId || !biasGames.TryGet(Context.User.Id, out BiasGameData? data))
            {
                await RespondAsync("You are not the owner of this interaction.", ephemeral: true);
                return;
            }

            //A click must always be acknowledged within 3 seconds, otherwise Discord shows "This interaction failed"
            if (data.IsProcessing)
            {
                await DeferAsync();
                return;
            }

            data.IsProcessing = true;

            data.SetDebut(chosenYears);

            logger.Log($"BiasGame Setup Debut Chosen: {string.Join(", ", chosenYears)}", LogOnly: true);

            IComponentInteraction component = (IComponentInteraction) Context.Interaction;

            ComponentBuilder waitButton = new ComponentBuilder()
                .WithButton("Your game is being set up...", "BiasGame_Setup_Debut_Disabled", disabled: true, style: ButtonStyle.Secondary);

            await component.UpdateAsync(x => x.Components = waitButton.Build());

            if (!await CreatePolaroids(data))
            {
                return;
            }

            int[] idolIds = data.Pairs[data.CurrentPair];

            logger.Log($"Creating combined image from idols. (ID 1: {idolIds[0]}, ID 2: {idolIds[1]})");
            using (Stream combined = BiasGameImageProcessor.CombineImages(
                (MemoryStream) data.IdolWithImage[idolIds[0]].Stream,
                (MemoryStream) data.IdolWithImage[idolIds[1]].Stream))
            using (FileAttachment file = new(combined, "combined.png"))
            {
                Embed[] embeds = BiasGameEmbedProcessor.CreateEmbed(
                    data,
                    GetCurrentUserAvatar(),
                    GetCurrentUserNickname());

                MessageComponent components = BiasGameEmbedProcessor.CreateComponent(idolIds, Context.User.Id);

                await DeleteOriginalResponseAsync();

                //Followup will respond with the first embed
                IUserMessage message = await FollowupWithFileAsync(file, embeds: embeds, components: components);
                data.MessageId = message.Id;
                data.IsProcessing = false;
            }
        }
        catch (Exception ex)
        {
            logger.Error("BiasGameInteraction.cs DebutChosen", ex);
            biasGames.Remove(Context.User.Id);
            await RespondOrFollowupAsync("Failure during setup!");
        }
    }

    //Returns false if the game cannot start, in that case the user is already notified
    private async Task<bool> CreatePolaroids(BiasGameData data)
    {
        List<IdolGameResource> idols = await idolService.GetListForGameAsync(data.Gender, data.DebutYearStart, data.DebutYearEnd);

        if (idols.Count < 16)
        {
            await DeleteOriginalResponseAsync();

            _ = await FollowupAsync("Not enough idols with your selected parameters!");
            biasGames.Remove(Context.User.Id);
            return false;
        }

        foreach (IdolGameResource idol in idols)
        {
            logger.Log($"Creating Polaroid for idol. (ID: {idol.IdolId})");
            Stream stream = await biasGameImageProcessor.CreatePolaroid(idol);
            string fileName = $"{Guid.NewGuid()}_{idol.IdolId}.png";

            data.AddImage(idol.IdolId, stream, fileName);
        }

        data.CreatePairs();
        return true;
    }
}
