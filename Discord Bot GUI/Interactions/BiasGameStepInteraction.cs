using Discord;
using Discord.Interactions;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions;

public class BiasGameStepInteraction(
    IIdolService idolService,
    BiasGameWinnerBracketImageProcessor biasGameWinnerBracketImageProcessor,
    IUserIdolStatisticService userIdolStatisticService,
    BotLogger logger,
    Config config) : BaseInteraction(logger, config)
{
    private readonly IIdolService idolService = idolService;
    private readonly BiasGameWinnerBracketImageProcessor biasGameWinnerBracketImageProcessor = biasGameWinnerBracketImageProcessor;
    private readonly IUserIdolStatisticService userIdolStatisticService = userIdolStatisticService;

    [ComponentInteraction("BiasGame_Next_*_*")]
    public async Task BiasGameNext(int idolId, ulong userId)
    {
        try
        {
            if (Context.User.Id != userId || !Global.BiasGames.TryGetValue(Context.User.Id, out BiasGameData data))
            {
                await RespondAsync("You are not the owner of this interaction.", ephemeral: true);
                return;
            }

            if (data.IsProcessing)
            {
                return;
            }

            data.IsProcessing = true;

            await DeferAsync();
            logger.Log($"BiasGame Next Step: IdolId: {idolId}", LogOnly: true);

            data.WinnerBracket = biasGameWinnerBracketImageProcessor.UpdateWinnerBracket(data);
            data.RemoveItem(idolId);

            if (data.IdolWithImage.Count == 1)
            {
                await FinalizeGame(data);
                return;
            }

            //New round
            if (data.CurrentPair > data.Pairs.Count - 1)
            {
                data.CreatePairs();
            }

            int[] idolIds = data.Pairs[data.CurrentPair];
            List<FileAttachment> files = [
                data.IdolWithImage[idolIds[0]],
                data.IdolWithImage[idolIds[1]]
            ];

            logger.Log($"Creating combined image from idols. (ID 1: {idolIds[0]}, ID 2: {idolIds[1]})");
            Stream combined = BiasGameImageProcessor.CombineImages((MemoryStream) files[0].Stream, (MemoryStream) files[1].Stream);
            files = [new FileAttachment(combined, "combined.png")];

            Embed[] embeds = BiasGameEmbedProcessor.CreateEmbed(
                data,
                GetCurrentUserAvatar(),
                GetCurrentUserNickname());

            MessageComponent components = BiasGameEmbedProcessor.CreateComponent(idolIds, Context.User.Id);

            await ModifyOriginalResponseAsync(x =>
            {
                x.Attachments = files;
                x.Embeds = embeds;
                x.Components = components;
            });

            data.IsProcessing = false;
        }
        catch (Exception ex)
        {
            logger.Error("BiasGameInteraction.cs DebutChosen", ex);
            Global.BiasGames.TryRemove(Context.User.Id, out _);
            await FollowupAsync("Failure during preparing next step!");
        }
    }

    private async Task FinalizeGame(BiasGameData data)
    {
        data.FinalizeData();

        IdolGameResource idolResource = await idolService.GetIdolByIdAsync(data.IdolWithImage.Keys.First());
        data.WinnerBracket = biasGameWinnerBracketImageProcessor.AddFinal(data, idolResource);
        List<FileAttachment> file = [new FileAttachment(data.WinnerBracket, "winner-bracket.png")];
        Embed[] embed = BiasGameEmbedProcessor.CreateFinalEmbed(
            data,
            GetCurrentUserAvatar(),
            GetCurrentUserNickname());

        await ModifyOriginalResponseAsync(x =>
        {
            x.Attachments = file;
            x.Embeds = embed;
            x.Components = null;
        });

        await userIdolStatisticService.UpdateUserStatisticsAsync(data.UserId, data.Ranking);
        Global.BiasGames.TryRemove(data.UserId, out _);
    }
}
