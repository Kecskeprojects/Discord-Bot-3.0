using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Config;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Processors.ImageProcessors;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions
{
    public class BiasGameInteraction(
        IIdolService idolService,
        BiasGameImageProcessor biasGameImageProcessor,
        BiasGameWinnerBracketImageProcessor biasGameWinnerBracketImageProcessor,
        IUserIdolStatisticService userIdolStatisticService,
        Logging logger,
        Config config) : BaseInteraction(logger, config)
    {
        private readonly IIdolService idolService = idolService;
        private readonly BiasGameImageProcessor biasGameImageProcessor = biasGameImageProcessor;
        private readonly BiasGameWinnerBracketImageProcessor biasGameWinnerBracketImageProcessor = biasGameWinnerBracketImageProcessor;
        private readonly IUserIdolStatisticService userIdolStatisticService = userIdolStatisticService;

        //Todo: select/input for when they were born
        [ComponentInteraction("BiasGame_Setup_Gender_*_*")]
        public async Task GenderChosen(GenderChoiceEnum choiceId, ulong userId)
        {
            try
            {
                if (Context.User.Id != userId || !Global.BiasGames.TryGetValue(Context.User.Id, out BiasGameData data))
                {
                    return;
                }

                data.SetGender(choiceId);

                logger.Log($"BiasGame Setup Gender Chosen: {choiceId}", LogOnly: true);

                List<int> options = [1995, 2000, 2005];
                for (int i = 2010; i <= DateTime.UtcNow.Year; i += 4)
                {
                    options.Add(i);
                }

                if ((DateTime.UtcNow.Year - 2010) % 4 > 0)
                {
                    options.Add(DateTime.UtcNow.Year);
                }

                SelectMenuBuilder selectMenu = new();
                selectMenu.WithCustomId($"BiasGame_Setup_Debut_{data.UserId}");
                selectMenu.WithPlaceholder("Select TWO years as a start and end date!");
                selectMenu.WithMinValues(2);
                selectMenu.WithMaxValues(2);

                options.ForEach(y => selectMenu.AddOption(y.ToString(), y.ToString()));

                ComponentBuilder components = new();
                components.WithSelectMenu(selectMenu);

                SocketMessageComponent component = Context.Interaction as SocketMessageComponent;
                await component.UpdateAsync(x => x.Components = components.Build());
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameInteraction.cs GenderChoosen", ex.ToString());
                Global.BiasGames.TryRemove(Context.User.Id, out _);
                await RespondAsync("Failure during setup!");
            }
        }

        [ComponentInteraction("BiasGame_Setup_Debut_*")]
        public async Task DebutChosen(ulong userId, string[] chosenYears)
        {
            try
            {
                if (Context.User.Id != userId || !Global.BiasGames.TryGetValue(Context.User.Id, out BiasGameData data))
                {
                    return;
                }

                data.SetDebut(chosenYears);

                logger.Log($"BiasGame Setup Debut Chosen: {string.Join(", ", chosenYears)}", LogOnly: true);

                SocketMessageComponent component = Context.Interaction as SocketMessageComponent;

                ComponentBuilder waitButton = new ComponentBuilder()
                    .WithButton("Your game is being set up...", "BiasGame_Setup_Debut_Disabled", disabled: true, style: ButtonStyle.Secondary);
                await component.UpdateAsync(x => x.Components = waitButton.Build());

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

                int[] idolIds = data.Pairs[data.CurrentPair];
                List<FileAttachment> files = [
                    data.IdolWithImage[idolIds[0]],
                    data.IdolWithImage[idolIds[1]]
                ];

                Stream combined = biasGameImageProcessor.CombineImages((MemoryStream)files[0].Stream, (MemoryStream)files[1].Stream);
                files = [new FileAttachment(combined, "combined.png")];

                Embed[] embeds = CreateEmbeds(data);

                ComponentBuilder components = CreateButtons(idolIds);

                await DeleteOriginalResponseAsync();

                //Followup will respond with the first embed
                await FollowupWithFilesAsync(files, embeds: embeds, components: components.Build());
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameInteraction.cs DebutChosen", ex.ToString());
                Global.BiasGames.TryRemove(Context.User.Id, out _);
                await RespondAsync("Failure during setup!");
            }
        }

        [ComponentInteraction("BiasGame_Next_*_*")]
        public async Task BiasGameNext(int idolId, ulong userId)
        {
            try
            {
                if (Context.User.Id != userId || !Global.BiasGames.TryGetValue(Context.User.Id, out BiasGameData data))
                {
                    return;
                }

                logger.Log($"BiasGame Next Step: IdolId: {idolId}", LogOnly: true);

                data.WinnerBracket = biasGameWinnerBracketImageProcessor.UpdateWinnerBracket(data);
                data.RemoveItem(idolId);

                SocketMessageComponent component = Context.Interaction as SocketMessageComponent;
                if (data.IdolWithImage.Count == 1)
                {
                    data.FinalizeData();
                    data.WinnerBracket = biasGameWinnerBracketImageProcessor.AddFinal(data);
                    List<FileAttachment> file = [new FileAttachment(data.WinnerBracket, "winner-bracket.png")];
                    Embed[] embed = CreateFinalEmbed(data);

                    await component.UpdateAsync(x =>
                    {
                        x.Attachments = file;
                        x.Embeds = embed;
                        x.Components = null;
                    });

                    await userIdolStatisticService.UpdateUserStatisticsAsync(data.UserId, data.Ranking);
                    Global.BiasGames.TryRemove(data.UserId, out _);
                    return;
                }

                if (data.CurrentPair > data.Pairs.Count - 1)
                {
                    data.CreatePairs();
                }

                int[] idolIds = data.Pairs[data.CurrentPair];
                List<FileAttachment> files = [
                    data.IdolWithImage[idolIds[0]],
                    data.IdolWithImage[idolIds[1]]
                ];

                Stream combined = biasGameImageProcessor.CombineImages((MemoryStream)files[0].Stream, (MemoryStream)files[1].Stream);
                files = [new FileAttachment(combined, "combined.png")];

                Embed[] embeds = CreateEmbeds(data);

                ComponentBuilder components = CreateButtons(idolIds);

                await component.UpdateAsync(x =>
                {
                    x.Attachments = files;
                    x.Embeds = embeds;
                    x.Components = components.Build();
                });
            }
            catch (Exception ex)
            {
                logger.Error("BiasGameInteraction.cs DebutChosen", ex.ToString());
                Global.BiasGames.TryRemove(Context.User.Id, out _);
                await RespondAsync("Failure during setup!");
            }
        }

        private ComponentBuilder CreateButtons(int[] idolIds)
        {
            //The idol IDs are reversed, the selected button sends the idol to delete from the list
            ActionRowBuilder buttonRow = new();
            buttonRow.WithButton(label: "\U000025C4", customId: $"BiasGame_Next_{idolIds[1]}_{Context.User.Id}", style: ButtonStyle.Primary); //Left Arrow 
            buttonRow.WithButton(label: "Who do you pick?", customId: $"BiasGame_Next_Disabled", disabled: true, style: ButtonStyle.Secondary);
            buttonRow.WithButton(label: "\U000025BA", customId: $"BiasGame_Next_{idolIds[0]}_{Context.User.Id}", style: ButtonStyle.Primary); //Right Arrow

            ComponentBuilder components = new();
            components.AddRow(buttonRow);

            return components;
        }

        private Embed[] CreateEmbeds(BiasGameData data)
        {
            List<Embed> embeds = [];

            EmbedBuilder main = new();

            main.WithDescription($"**BIAS GAME MATCH {data.CurrentPair + 1} OUT OF {data.Pairs.Count}**");

            EmbedFooterBuilder footer = new();
            footer.WithIconUrl(Context.User.GetDisplayAvatarUrl(ImageFormat.Jpeg, 512));
            footer.WithText($"{Global.GetNickName(Context.Channel, Context.User)} | {data.Gender} | {data.DebutYearStart}-{data.DebutYearEnd}");
            main.WithFooter(footer);

            main.WithImageUrl($"attachment://combined.png");

            embeds.Add(main.Build());

            return [.. embeds];
        }

        private Embed[] CreateFinalEmbed(BiasGameData data)
        {
            List<Embed> embeds = [];

            EmbedBuilder main = new();

            main.WithDescription("**BIAS GAME MATCH RESULT**");

            EmbedFooterBuilder footer = new();
            footer.WithIconUrl(Context.User.GetDisplayAvatarUrl(ImageFormat.Jpeg, 512));
            footer.WithText($"{Global.GetNickName(Context.Channel, Context.User)} | {data.Gender} | {data.DebutYearStart}-{data.DebutYearEnd}");
            main.WithFooter(footer);
            embeds.Add(main.WithImageUrl("attachment://winner-bracket.png").Build());

            return [.. embeds];
        }
    }
}
