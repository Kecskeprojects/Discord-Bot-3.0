namespace Discord_Bot.Communication.LastFm;

public class ArtistStats
{
    public string AlbumField { get; set; } = "";
    public string TrackField { get; set; } = "";
    public int Playcount { get; set; }
    //Null when neither Spotify nor Last.fm had an image
    public string? ImageUrl { get; set; }
    //Null when the stats were found, else what the user is told instead
    public string? Message { get; set; }
    public string ArtistName { get; set; } = "";
}
