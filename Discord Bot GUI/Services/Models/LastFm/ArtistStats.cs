namespace Discord_Bot.Services.Models.LastFm;

public class ArtistStats(string username)
{
    public string Username { get; set; } = username;

    public string AlbumField { get; set; } = "";

    public string TrackField { get; set; } = "";

    public int Playcount { get; set; } = 0;

    public string ImageUrl { get; set; }
    public string Message { get; set; }
    public string ArtistName { get; set; }
    public int AlbumCount { get; set; }
    public int TrackCount { get; set; }
}
