using Discord;
using Discord_Bot.Communication.Bias;

namespace Discord_Bot.Processors.EmbedProcessors.BiasGame
{
    public static class BiasGameEmbedProcessor
    {
        public static MessageComponent CreateComponent(int[] idolIds, ulong userId)
        {
            //The idol IDs are reversed, the selected button sends the idol to delete from the list
            ActionRowBuilder buttonRow = new();
            buttonRow.WithButton(label: "\U000025C4", customId: $"BiasGame_Next_{idolIds[1]}_{userId}", style: ButtonStyle.Primary); //Left Arrow 
            buttonRow.WithButton(label: "Who do you pick?", customId: $"BiasGame_Next_Disabled", disabled: true, style: ButtonStyle.Secondary);
            buttonRow.WithButton(label: "\U000025BA", customId: $"BiasGame_Next_{idolIds[0]}_{userId}", style: ButtonStyle.Primary); //Right Arrow

            ComponentBuilder components = new();
            components.AddRow(buttonRow);

            return components.Build();
        }

        public static Embed[] CreateEmbed(BiasGameData data, string avatarUrl, string userName)
        {
            EmbedBuilder main = new();

            main.WithDescription($"**BIAS GAME MATCH {data.CurrentPair + 1} OUT OF {data.Pairs.Count}**");

            EmbedFooterBuilder footer = new();
            footer.WithIconUrl(avatarUrl);
            footer.WithText($"{userName} | {data.Gender} | {data.DebutYearStart}-{data.DebutYearEnd}");
            main.WithFooter(footer);

            main.WithImageUrl($"attachment://combined.png");

            return [main.Build()];
        }

        public static Embed[] CreateFinalEmbed(BiasGameData data, string avatarUrl, string userName)
        {
            EmbedBuilder main = new();

            main.WithDescription("**BIAS GAME MATCH RESULT**");

            EmbedFooterBuilder footer = new();
            footer.WithIconUrl(avatarUrl);
            footer.WithText($"{userName} | {data.Gender} | {data.DebutYearStart}-{data.DebutYearEnd}");
            main.WithFooter(footer);
            main.WithImageUrl("attachment://winner-bracket.png");

            return [main.Build()];
        }
    }
}
