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

    //One page of a user's top artists by name
    public static string TopArtistsNamed(params (string Name, int Plays)[] artists)
    {
        JsonNode?[] items = [.. artists.Select((artist, i) => new JsonObject
        {
            ["name"] = artist.Name,
            ["playcount"] = artist.Plays.ToString(),
            ["mbid"] = "",
            ["url"] = "https://www.last.fm/music/artist",
            ["image"] = Images(),
            ["@attr"] = new JsonObject { ["rank"] = (i + 1).ToString() }
        })];

        return new JsonObject { ["topartists"] = new JsonObject { ["artist"] = new JsonArray(items), ["@attr"] = Page(1, 1) } }.ToJsonString();
    }

    //One page of a user's top albums
    public static string TopAlbums(params (string Name, string Artist, int Plays)[] albums)
    {
        JsonNode?[] items = [.. albums.Select((album, i) => new JsonObject
        {
            ["name"] = album.Name,
            ["playcount"] = album.Plays.ToString(),
            ["mbid"] = "",
            ["url"] = "https://www.last.fm/music/album",
            ["artist"] = new JsonObject { ["name"] = album.Artist, ["mbid"] = "", ["url"] = "https://www.last.fm/music/artist" },
            ["image"] = Images(),
            ["@attr"] = new JsonObject { ["rank"] = (i + 1).ToString() }
        })];

        return new JsonObject { ["topalbums"] = new JsonObject { ["album"] = new JsonArray(items), ["@attr"] = Page(1, 1) } }.ToJsonString();
    }

    //A user's latest scrobbles, newest first; the first is the one playing now when nowPlaying is set, the others have a date
    public static string RecentTracks(bool nowPlaying, params (string Name, string Artist, string Album)[] tracks)
    {
        JsonNode?[] items = [.. tracks.Select((track, i) =>
        {
            JsonObject item = new()
            {
                ["name"] = track.Name,
                ["mbid"] = "",
                ["url"] = "https://www.last.fm/music/track",
                ["streamable"] = "0",
                ["artist"] = new JsonObject { ["mbid"] = "", ["#text"] = track.Artist },
                ["album"] = new JsonObject { ["mbid"] = "", ["#text"] = track.Album },
                ["image"] = Images()
            };
            if (nowPlaying && i == 0)
            {
                item["@attr"] = new JsonObject { ["nowplaying"] = "true" };
            }
            else
            {
                item["date"] = new JsonObject { ["uts"] = "1795442400", ["#text"] = "18 Nov 2026, 14:00" };
            }
            return item;
        })];

        return new JsonObject { ["recenttracks"] = new JsonObject { ["track"] = new JsonArray(items), ["@attr"] = Page(1, 1) } }.ToJsonString();
    }

    //track.getInfo asked with a username: the track and that user's plays of it
    public static string TrackInfo(string name, string artist, int userPlays)
    {
        return new JsonObject
        {
            ["track"] = new JsonObject
            {
                ["name"] = name,
                ["mbid"] = "",
                ["url"] = "https://www.last.fm/music/track",
                ["listeners"] = "1000",
                ["playcount"] = "5000",
                ["artist"] = new JsonObject { ["name"] = artist, ["mbid"] = "", ["url"] = "https://www.last.fm/music/artist" },
                ["userplaycount"] = userPlays.ToString()
            }
        }.ToJsonString();
    }

    //artist.getInfo asked with a username: the artist and that user's plays of them
    public static string ArtistInfo(string name, int userPlays)
    {
        return new JsonObject
        {
            ["artist"] = new JsonObject
            {
                ["name"] = name,
                ["mbid"] = "",
                ["url"] = "https://www.last.fm/music/artist",
                ["image"] = Images(),
                ["stats"] = new JsonObject { ["listeners"] = "1000", ["playcount"] = "5000", ["userplaycount"] = userPlays.ToString() }
            }
        }.ToJsonString();
    }

    //A responder for FakeLastFm: each request gets the response under "method:user" (user or username in the query),
    //else under "method"; a method with neither gets Last.fm's error for an unknown method
    public static Func<Uri, string> Router(Dictionary<string, string> responses)
    {
        return uri =>
        {
            System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            string method = query["method"] ?? "";
            string user = query["user"] ?? query["username"] ?? "";
            if (responses.TryGetValue($"{method}:{user}", out string? forUser))
            {
                return forUser;
            }
            return responses.TryGetValue(method, out string? forAnyone) ? forAnyone : Error(3, "Invalid Method - No method with that name in this package");
        };
    }

    public static string Error(int code, string message)
    {
        return new JsonObject { ["error"] = code, ["message"] = message }.ToJsonString();
    }

    //Last.fm lists its images from small to extra large; the bot takes the last
    private static JsonArray Images()
    {
        return new JsonArray(
            new JsonObject { ["size"] = "small", ["#text"] = "https://example.com/small.png" },
            new JsonObject { ["size"] = "extralarge", ["#text"] = "https://example.com/cover.png" });
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
