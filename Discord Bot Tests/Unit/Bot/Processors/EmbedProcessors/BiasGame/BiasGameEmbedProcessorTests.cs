using DatabaseORM.Enums;
using Discord;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Processors.EmbedProcessors.BiasGame;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;
using System.IO;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.BiasGame;

//A match of two idols, and the result at the end of the game
public sealed class BiasGameEmbedProcessorTests
{
    //Picking one idol sends the other one's ID: the game removes the loser
    [Fact]
    public void CreateComponent_EachArrowSendsTheOtherIdol()
    {
        List<ButtonComponent> buttons = ComponentParts.Buttons(BiasGameEmbedProcessor.CreateComponent([11, 12], DiscordIds.User));

        Assert.Equal($"BiasGame_Next_12_{DiscordIds.User}", buttons[0].CustomId);
        Assert.True(buttons[1].IsDisabled);
        Assert.Equal($"BiasGame_Next_11_{DiscordIds.User}", buttons[2].CustomId);
    }

    [Fact]
    public void CreateEmbed_ShowsTheMatchAndTheGamesSettings()
    {
        using (BiasGameData data = Game(idols: 4))
        {
            Embed embed = Assert.Single(BiasGameEmbedProcessor.CreateEmbed(data, TestValues.AvatarUrl, TestValues.Username));

            Assert.Equal("**BIAS GAME MATCH 1 OUT OF 2**", embed.Description);
            Assert.Equal($"{TestValues.Username} | Female | 2010-2020", embed.Footer?.Text);
            Assert.Equal(TestValues.AvatarUrl, embed.Footer?.IconUrl);
            Assert.Equal("attachment://combined.png", embed.Image?.Url);
        }
    }

    [Fact]
    public void CreateFinalEmbed_ShowsTheWinnerBracket()
    {
        using (BiasGameData data = Game(idols: 2))
        {
            Embed embed = Assert.Single(BiasGameEmbedProcessor.CreateFinalEmbed(data, TestValues.AvatarUrl, TestValues.Username));

            Assert.Equal("**BIAS GAME MATCH RESULT**", embed.Description);
            Assert.Equal("attachment://winner-bracket.png", embed.Image?.Url);
        }
    }

    //Debut years given the wrong way round, as a user can pick them
    private static BiasGameData Game(int idols)
    {
        BiasGameData data = new(DiscordIds.User, TestTimes.Now.UtcDateTime) { Gender = GenderEnum.Female };
        data.SetDebut(["2020", "2010"]);
        for (int id = 1; id <= idols; id++)
        {
            data.AddImage(id, new MemoryStream(), $"{id}.png");
        }
        data.CreatePairs();
        return data;
    }
}
