using DatabaseORM.Interface;
using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Interfaces.Processors;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Services.Models.LastFm;
using System.IO;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;

public class LastFmWhoKnowsEmbedProcessor(IWhoKnowsImageProcessor whoKnowsImageProcessor, IWebService webService, IBotLogger logger) : LastFmBaseEmbedProcessor
{
    private readonly IWhoKnowsImageProcessor whoKnowsImageProcessor = whoKnowsImageProcessor;
    private readonly IWebService webService = webService;
    private readonly IBotLogger logger = logger;

    public async Task<WhoKnowsEmbedResult> CreateEmbed(WhoKnows wk)
    {
        WhoKnowsEmbedResult result = new();
        //Getting base of lastfm embed
        EmbedBuilder builder = GetBaseEmbedBuilder($"Server ranking for:\n{wk.EmbedTitle}");

        if (!string.IsNullOrEmpty(wk.ImageUrl))
        {
            //Download image and get back it's filepath
            logger.Query($"Getting album cover image:\n{wk.ImageUrl}");
            using (MemoryStream originalImage = await webService.GetStream(wk.ImageUrl))
            {
                //Edit the picture to the list format
                EditPictureResult? modifiedImage = whoKnowsImageProcessor.EditPicture(originalImage, wk.Plays, wk.EmbedTitle);

                if (modifiedImage != null)
                {
                    result.ImageData = modifiedImage.Stream;
                    result.ImageName = modifiedImage.FileName;
                }
            }
        }

        if (result.ImageData != null)
        {
            //Add image reference to the embed
            _ = builder.WithImageUrl($"attachment://{result.ImageName}");

            result.HasImage = true;
            result.Embed = [builder.Build()];
        }
        else
        {
            CreateBasicEmbed(wk, builder);

            result.HasImage = false;
            result.Embed = [builder.Build()];
        }
        return result;
    }

    private static void CreateBasicEmbed(WhoKnows wk, EmbedBuilder builder)
    {
        string[] list = ["", "", ""];
        int i = 1;
        int index = 0;
        //Three fields of 15, so the list stops at 45 listeners (a 46th threw until 2026-11-23)
        foreach (KeyValuePair<string, int> userplays in wk.Plays.Take(list.Length * 15))
        {
            //One line in embed
            list[index] += $"`#{i}` **{userplays.Key}** with *{userplays.Value} plays*";
            list[index] += "\n";

            //If we went through 15 results, start filling a new list page
            if (i % 15 == 0)
            {
                index++;
            }

            i++;
        }

        //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
        foreach (string item in list)
        {
            if (item != "")
            {
                _ = builder.AddField("\u200b", item, false);
            }
        }

        if (wk.Plays.Count > list.Length * 15)
        {
            _ = builder.WithFooter($"{wk.Plays.Count - (list.Length * 15)} more not shown");
        }
    }
}
