using System.Text.Json.Nodes;

namespace Discord_Bot.Tests.Mocks.Http;

/// <summary>
/// Last.fm answers in the shape the real API sends, with the fields the bot reads. Responses recorded once from the real
/// API would be closer still (see "Saved responses" in the improvement notes).
/// </summary>
public static class LastFmResponses
{
    public static string TopTracks(params (string Name, string Artist, int Plays)[] tracks)
    {
        JsonNode?[] items = [.. tracks.Select((track, i) => new JsonObject
        {
            ["name"] = track.Name,
            ["playcount"] = track.Plays.ToString(),
            ["mbid"] = "",
            ["url"] = "https://www.last.fm/music/track",
            ["artist"] = new JsonObject { ["name"] = track.Artist, ["mbid"] = "", ["url"] = "https://www.last.fm/music/artist" },
            ["image"] = new JsonArray(new JsonObject { ["size"] = "small", ["#text"] = "https://example.com/cover.png" }),
            ["@attr"] = new JsonObject { ["rank"] = (i + 1).ToString() }
        })];

        return new JsonObject { ["toptracks"] = new JsonObject { ["track"] = new JsonArray(items), ["@attr"] = Page(1, 1) } }.ToJsonString();
    }

    //One page of a user's top artists; only the play counts matter to the bot's totals
    public static string TopArtists(int page, int totalPages, params int[] plays)
    {
        JsonNode?[] items = [.. plays.Select((count, i) => new JsonObject
        {
            ["name"] = $"Artist {page}-{i}",
            ["playcount"] = count.ToString(),
            ["mbid"] = "",
            ["url"] = "https://www.last.fm/music/artist",
            ["@attr"] = new JsonObject { ["rank"] = (i + 1).ToString() }
        })];

        return new JsonObject { ["topartists"] = new JsonObject { ["artist"] = new JsonArray(items), ["@attr"] = Page(page, totalPages) } }.ToJsonString();
    }

    public static string Error(int code, string message)
    {
        return new JsonObject { ["error"] = code, ["message"] = message }.ToJsonString();
    }

    private static JsonObject Page(int page, int totalPages)
    {
        return new JsonObject
        {
            ["user"] = "lastfm-tester",
            ["page"] = page.ToString(),
            ["totalPages"] = totalPages.ToString(),
            ["perPage"] = "1000",
            ["total"] = "0"
        };
    }
}
