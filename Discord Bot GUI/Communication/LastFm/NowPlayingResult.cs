namespace Discord_Bot.Communication.LastFm;

public class NowPlayingResult
{
    //Null when neither Spotify nor Last.fm had an image
    public string? ImageUrl { get; set; }
    //Null when the track was found, else what the user is told instead
    public string? Message { get; set; }
    //Null when Last.fm didn't give the user's plays of the track
    public string? TrackPlays { get; set; }
    //Null when the track isn't in the user's top tracks this month
    public string? Ranking { get; set; }
    public bool NowPlaying { get; set; }
    public string TrackName { get; set; } = "";
    public string ArtistName { get; set; } = "";
    public string AlbumName { get; set; } = "";
    public string Url { get; set; } = "";
    public string ArtistMbid { get; set; } = "";

    //Null after a single scrobble, when there is no previous track
    public string? SecondTrackName { get; set; }
    public string? SecondTrackArtist { get; set; }
}
