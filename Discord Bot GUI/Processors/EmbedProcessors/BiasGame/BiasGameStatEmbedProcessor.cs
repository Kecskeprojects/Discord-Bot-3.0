using Discord;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Linq;

namespace Discord_Bot.Processors.EmbedProcessors.BiasGame;

public static class BiasGameStatEmbedProcessor
{
    public static Embed[] CreateEmbed(string nickName, GenderEnum gender, UserBiasGameStatResource stats)
    {
        EmbedBuilder builder = new();
        string heartEmoji = ":heart:";

        builder.WithTitle($"Bias Game Stats for {nickName}");
        builder.WithDescription(
            "Data is weighed based on how high an idol got in the leader board in your different games, with winning adding the most weight.\n" +
            "Use the buttons to change the gender the list is based on.\n" +
            $"{heartEmoji} emoji means the idol is also added as your bias.");

        builder.WithThumbnailUrl(stats.Stats.First().LatestImageUrl);
        builder.WithCurrentTimestamp();
        builder.WithFooter($"Gender: {gender.ToDatabaseFriendlyString()}");

        string list = "";
        for (int i = 0; i < stats.Stats.Count; i++)
        {
            string heart = stats.Stats[i].IsUserBias ? $" {heartEmoji}" : "";
            list += $"#{i + 1} {stats.Stats[i].IdolStageName} - {stats.Stats[i].IdolGroupFullName.Replace("*", "\\*")}{heart}\n";
        }

        builder.AddField($"Your Favorites from *{stats.BiasGameCount} games*", list);

        return [builder.Build()];
    }

    public static MessageComponent CreateComponent(GenderEnum currentGender, ulong userId)
    {
        ActionRowBuilder buttonRow = new();
        buttonRow.WithButton(
            emote: new Emoji("\U0001F57A"), //Man
            customId: $"BiasStats_Gender_1_{currentGender}_{userId}",
            style: ButtonStyle.Primary);
        buttonRow.WithButton(
            emote: new Emoji("\U0001F483"), //Woman
            customId: $"BiasStats_Gender_2_{currentGender}_{userId}",
            style: ButtonStyle.Primary);
        buttonRow.WithButton(
            emote: new Emoji("\U0001F46B"), //Both
            customId: $"BiasStats_Gender_3_{currentGender}_{userId}",
            style: ButtonStyle.Primary);

        ComponentBuilder components = new();
        components.AddRow(buttonRow);

        return components.Build();
    }
}
