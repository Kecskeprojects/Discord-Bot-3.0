using Discord;

namespace Discord_Bot.Processors.EmbedProcessors
{
    public class BiasGameGenderEmbedProcessor
    {
        public static Embed[] CreateEmbed(string userName, string avatarUrl)
        {
            EmbedBuilder mainEmbed = new();
            mainEmbed.WithTitle("Bias Game Setup");

            EmbedFooterBuilder footer = new();
            footer.WithIconUrl(avatarUrl);
            footer.WithText(userName);
            mainEmbed.WithFooter(footer);

            mainEmbed.AddField("1. Select a gender", "Male, Female, Both");
            mainEmbed.AddField("2. Select a debut range", "A start date and an end date");
            return [mainEmbed.Build()];
        }

        public static MessageComponent CreateComponent(ulong userId)
        {
            ActionRowBuilder buttonRow = new();
            buttonRow.WithButton(
                emote: new Emoji("\U0001F57A"), //Man
                customId: $"BiasGame_Setup_Gender_1_{userId}",
                style: ButtonStyle.Primary);
            buttonRow.WithButton(
                emote: new Emoji("\U0001F483"), //Woman
                customId: $"BiasGame_Setup_Gender_2_{userId}",
                style: ButtonStyle.Primary);
            buttonRow.WithButton(
                emote: new Emoji("\U0001F46B"), //Both
                customId: $"BiasGame_Setup_Gender_3_{userId}",
                style: ButtonStyle.Primary);

            ComponentBuilder components = new();
            components.AddRow(buttonRow);
            return components.Build();
        }
    }
}
