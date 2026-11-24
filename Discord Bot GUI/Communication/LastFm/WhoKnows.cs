using DatabaseORM.Resources;

namespace Discord_Bot.Communication.LastFm;

public class WhoKnows(List<UserResource> users)
{
    public List<UserResource> Users { get; set; } = users;
    public Dictionary<string, int> Plays { get; set; } = [];
    public string EmbedTitle { get; set; } = "";
    //Null when neither Spotify nor Last.fm had an image
    public string? ImageUrl { get; set; }
    //Null when the ranking was made, else what the user is told instead
    public string? Message { get; set; }
}
