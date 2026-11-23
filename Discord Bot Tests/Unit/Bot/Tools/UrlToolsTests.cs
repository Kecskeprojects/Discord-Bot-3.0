using Discord_Bot.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Tools;

//Used by the Instagram and Twitter embeds, which are both disabled at the moment
public sealed class UrlToolsTests
{
    private const string Site = "https://instagram.com/";

    [Fact]
    public void LinkSearch_GivesEachLinkOfTheSiteWithoutItsQuery()
    {
        List<Uri>? links = UrlTools.LinkSearch("look https://www.instagram.com/p/ABC/?igsh=xyz and https://instagram.com/reel/DEF/", false, Site);

        Assert.NotNull(links);
        Assert.Equal(["https://instagram.com/p/ABC/", "https://instagram.com/reel/DEF/"], links.Select(link => link.OriginalString));
    }

    //Discord shows no preview for a link in angle brackets, so the user asked for none
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void LinkSearch_LinkInAngleBrackets_IsSkippedUnlessAsked(bool ignoreEmbedSuppress, int expected)
    {
        List<Uri>? links = UrlTools.LinkSearch("look <https://instagram.com/p/ABC/>", ignoreEmbedSuppress, Site);

        Assert.NotNull(links);
        Assert.Equal(expected, links.Count);
    }

    [Theory]
    [InlineData("just a message")]
    [InlineData("https://example.com/p/ABC/")]
    [InlineData("")]
    public void LinkSearch_NoLinkOfTheSite_GivesNull(string message)
    {
        Assert.Null(UrlTools.LinkSearch(message, false, Site));
    }

    [Fact]
    public void SanitizeText_PutsEachLinkInAngleBrackets()
    {
        Assert.Equal("see <https://a.com/x> and <https://b.com/y> now", UrlTools.SanitizeText("see https://a.com/x and https://b.com/y now"));
    }

    [Fact]
    public void SanitizeText_EmptyText_StaysEmpty()
    {
        Assert.Equal("", UrlTools.SanitizeText("  "));
    }
}
