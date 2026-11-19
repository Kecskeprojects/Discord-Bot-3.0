using Discord;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.BiasGame;

public sealed class BiasGameDebutEmbedProcessorTests
{
    [Theory]
    [InlineData(2026, "1990,2000,2010,2014,2018,2020,2022,2024,2026")]
    [InlineData(2027, "1990,2000,2010,2014,2018,2020,2022,2024,2026,2027")]
    public void CreateComponent_OffersEveryOtherYearSince2020AndTheCurrentOne(int currentYear, string expectedYears)
    {
        using (BiasGameData data = new(DiscordIds.User, TestTimes.Now.UtcDateTime))
        {
            MessageComponent component = BiasGameDebutEmbedProcessor.CreateComponent(data, currentYear);

            SelectMenuComponent menu = component.Components
                .OfType<ActionRowComponent>()
                .SelectMany(row => row.Components)
                .OfType<SelectMenuComponent>()
                .Single();
            Assert.Equal(expectedYears, string.Join(",", menu.Options.Select(option => option.Value)));
            Assert.Equal($"BiasGame_Setup_Debut_{DiscordIds.User}", menu.CustomId);
        }
    }
}
