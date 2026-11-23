using Discord_Bot.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Tools.Extensions;

public sealed class StringExtensionTests
{
    //The bias scraping compares group names with it
    [Theory]
    [InlineData("Group*Star! (2020)", "GroupStar 2020")]
    [InlineData("plain", "plain")]
    public void RemoveSpecialCharacters_KeepsLettersDigitsAndSpaces(string input, string expected)
    {
        Assert.Equal(expected, input.RemoveSpecialCharacters());
    }

    [Theory]
    [InlineData("https://a.com", "b", "https://a.com/b")]
    [InlineData("https://a.com/", "b", "https://a.com/b")]
    [InlineData("https://a.com/", "/b", "https://a.com/b")]
    public void UrlCombine_PutsOneSlashBetween(string start, string addition, string expected)
    {
        Assert.Equal(expected, start.UrlCombine(addition));
    }

    //The case of each letter is random, so this checks what every result keeps, over several runs
    [Fact]
    public void ToMockText_KeepsTheLettersAndNeverHasThreeCapitalsInARow()
    {
        const string text = "i think cats are great and dogs are too";

        for (int run = 0; run < 25; run++)
        {
            string mocked = text.ToMockText();

            Assert.Equal(text, mocked.ToLowerInvariant());
            Assert.DoesNotMatch("[A-Z]{3}", mocked);
        }
    }
}
