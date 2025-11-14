namespace LastFmApi.Communication;

public class WhoKnowsResponseItem
{
    public Dictionary<string, int> Plays { get; set; } = [];
    public string EmbedTitle { get; set; }
    public string ImageUrl { get; set; }
    public string ArtistMbid { get; set; }
    public string ArtistName { get; set; }
    public string TrackName { get; set; }
}
