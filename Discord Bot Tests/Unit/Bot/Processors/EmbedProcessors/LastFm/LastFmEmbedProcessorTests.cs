using System.IO;
using DatabaseORM.Interface;
using Discord;
using Discord_Bot.Communication;
using Discord_Bot.Interfaces.Processors;
using Discord_Bot.Interfaces.Services;
using Discord_Bot.Processors.EmbedProcessors.LastFm;
using Discord_Bot.Services.Models.LastFm;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors.LastFm;

//The !lf embeds: a list, now playing, an artist's stats, and the who-knows ranking as an image or as text
public sealed class LastFmEmbedProcessorTests
{
    private readonly IWhoKnowsImageProcessor imageProcessor = Substitute.For<IWhoKnowsImageProcessor>();
    private readonly IWebService webService = Substitute.For<IWebService>();

    [Fact]
    public void List_GivesEachFilledPartAsAFieldAndTheTotalBelow()
    {
        Embed embed = Assert.Single(LastFmListEmbedProcessor.CreateEmbed("Ali's Top Tracks...", new LastFmListResult { TotalPlays = 40, EmbedFields = ["first ten\n", "next ten\n", ""] }));

        Assert.Equal("Ali's Top Tracks...", embed.Author?.Name);
        Assert.Equal(["first ten\n", "next ten\n"], embed.Fields.Select(field => field.Value));
        Assert.Equal("Total plays: 40", embed.Footer?.Text);
    }

    [Fact]
    public void NowPlaying_GivesTheTrackThePreviousOneAndThePlaysAndRankingBelow()
    {
        Embed embed = Assert.Single(LastFmNowPlayingEmbedProcessor.CreateEmbed("Ali is currently listening to...", NowPlaying()));

        Assert.Equal("Shut Down", embed.Title);
        Assert.Equal(["By *BLACKPINK*", "__Previous Track__"], embed.Fields.Select(field => field.Name));
        Assert.Equal("*Supernova* By *aespa*", embed.Fields[1].Value);
        Assert.Equal("3rd listen • 2nd most played this month", embed.Footer?.Text);
    }

    //A single scrobble has no previous track; it showed as "** By **" until 2026-11-23
    [Fact]
    public void NowPlaying_NoPreviousTrack_LeavesItsFieldOut()
    {
        NowPlayingResult nowPlaying = NowPlaying();
        nowPlaying.SecondTrackName = null!;
        nowPlaying.SecondTrackArtist = null!;

        Embed embed = Assert.Single(LastFmNowPlayingEmbedProcessor.CreateEmbed("Ali is currently listening to...", nowPlaying));

        Assert.Equal(["By *BLACKPINK*"], embed.Fields.Select(field => field.Name));
    }

    [Fact]
    public void Artist_GivesThePlaysAndTheFilledLists()
    {
        Embed embed = Assert.Single(LastFmArtistEmbedProcessor.CreateEmbed("Ali's stats for BLACKPINK", new ArtistStats { Playcount = 40, AlbumField = "`#1` **Born Pink**  (*10 plays*)\n" }));

        Assert.Equal("You have listened to this artist **40** times", embed.Description);
        Assert.Equal(["Top Albums"], embed.Fields.Select(field => field.Name));
    }

    [Fact]
    public async Task WhoKnows_CoverFound_PointsTheEmbedAtTheRankingImage()
    {
        webService.GetStream("https://example.com/cover.png").Returns(_ => new MemoryStream([1, 2, 3]));
        imageProcessor.EditPicture(default!, default!, default!).ReturnsForAnyArgs(_ => new EditPictureResult("ranking.png", new MemoryStream([4, 5, 6])));

        using WhoKnowsEmbedResult result = await Processor().CreateEmbed(WhoKnows(2, "https://example.com/cover.png"));

        Assert.True(result.HasImage);
        Assert.Equal("ranking.png", result.ImageName);
        Embed embed = Assert.Single(result.Embed);
        Assert.Equal("attachment://ranking.png", embed.Image?.Url);
        Assert.Equal("Server ranking for:\nShut Down by BLACKPINK", embed.Author?.Name);
    }

    [Fact]
    public async Task WhoKnows_ImageCouldNotBeMade_ListsTheListenersInstead()
    {
        webService.GetStream(default!).ReturnsForAnyArgs(_ => new MemoryStream([1, 2, 3]));
        imageProcessor.EditPicture(default!, default!, default!).ReturnsForAnyArgs((EditPictureResult?) null);

        using WhoKnowsEmbedResult result = await Processor().CreateEmbed(WhoKnows(2, "https://example.com/cover.png"));

        Assert.False(result.HasImage);
        Assert.Equal("`#1` **Listener 1** with *2 plays*\n`#2` **Listener 2** with *1 plays*\n", Assert.Single(Assert.Single(result.Embed).Fields).Value);
    }

    //Three fields of 15 listeners; until 2026-11-23 a 46th threw
    [Fact]
    public async Task WhoKnows_MoreListenersThanTheListHolds_ShowsTheTopAndCountsTheRest()
    {
        using WhoKnowsEmbedResult result = await Processor().CreateEmbed(WhoKnows(50, null));

        Embed embed = Assert.Single(result.Embed);
        Assert.Equal(3, embed.Fields.Length);
        Assert.Equal(45, embed.Fields.Sum(field => field.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length));
        Assert.Equal("5 more not shown", embed.Footer?.Text);
    }

    private LastFmWhoKnowsEmbedProcessor Processor()
    {
        return new LastFmWhoKnowsEmbedProcessor(imageProcessor, webService, Substitute.For<IBotLogger>());
    }

    //Listeners with one play fewer each, the first with as many as there are listeners
    private static WhoKnows WhoKnows(int listeners, string? imageUrl)
    {
        return new WhoKnows([])
        {
            EmbedTitle = "Shut Down by BLACKPINK",
            ImageUrl = imageUrl,
            Plays = Enumerable.Range(1, listeners).ToDictionary(i => $"Listener {i}", i => listeners - i + 1)
        };
    }

    private static NowPlayingResult NowPlaying()
    {
        return new NowPlayingResult
        {
            NowPlaying = true,
            TrackName = "Shut Down",
            ArtistName = "BLACKPINK",
            AlbumName = "Born Pink",
            Url = "https://www.last.fm/music/track",
            SecondTrackName = "Supernova",
            SecondTrackArtist = "aespa",
            TrackPlays = "3rd",
            Ranking = "2nd"
        };
    }
}
