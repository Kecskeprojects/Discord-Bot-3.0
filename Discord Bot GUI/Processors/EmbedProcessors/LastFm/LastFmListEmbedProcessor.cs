using Discord;
using Discord_Bot.Services.Models.LastFm;

namespace Discord_Bot.Processors.EmbedProcessors.LastFm;
public class LastFmListEmbedProcessor : LastFmBaseEmbedProcessor
{
    public static Embed[] CreateEmbed(string titleText, LastFmListResult data)
    {
        //Getting base of lastfm embed
        EmbedBuilder builder = GetBaseEmbedBuilder(titleText, data.ImageUrl);
        builder.WithFooter("Total plays: " + data.TotalPlays);

        //Make each part of the text into separate fields, thus going around the 1024 character limit of a single field
        foreach (string item in data.EmbedFields)
        {
            if (item != "")
            {
                builder.AddField("\u200b", item, false);
            }
        }

        return [builder.Build()];
    }
}
