using Discord_Bot.Communication.LastFm;
using Discord_Bot.Tests.Common.Tools;
using Discord_Bot.Tools.LastFmTools;
using LastFmApi.Models.TopTrack;

namespace Discord_Bot.Tests.Unit.Bot.Tools.LastFmTools;

public sealed class LastFmListResultToolsTests
{
    [Fact]
    public void CreateTopTrackList_WritesThePercentageWithADot_UnderHungarianToo()
    {
        LastFmListResult result = new() { TotalPlays = 57 };

        using (new CultureScope("hu-HU"))
        {
            LastFmListResultTools.CreateTopTrackList(10, result, [Track("Song A", "Artist", "19")]);
        }

        Assert.Equal("`#1`**Song A** by **Artist** - *33.33%* (*19 plays*)\n", result.EmbedFields[0]);
    }

    [Fact]
    public void CreateTopTrackList_StartsANewPageAfterTenTracks()
    {
        LastFmListResult result = new() { TotalPlays = 110 };
        List<Track> tracks = [.. Enumerable.Range(1, 11).Select(i => Track($"Song {i}", "Artist", "10"))];

        LastFmListResultTools.CreateTopTrackList(11, result, tracks);

        Assert.Equal(10, result.EmbedFields[0].Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.StartsWith("`#11`**Song 11**", result.EmbedFields[1]);
    }

    private static Track Track(string name, string artist, string plays)
    {
        return new Track { Name = name, PlayCount = plays, Artist = new Artist { Name = artist } };
    }
}
