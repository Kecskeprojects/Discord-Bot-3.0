namespace Discord_Bot.Communication.LastFm;

public class LastFmListResult
{
    //Null when the list was made, else what the user is told instead
    public string? Message { get; set; }
    //Null when neither Spotify nor Last.fm had an image
    public string? ImageUrl { get; set; }
    public int TotalPlays { get; set; }
    public string[] EmbedFields { get; set; } = ["", "", ""];
}
