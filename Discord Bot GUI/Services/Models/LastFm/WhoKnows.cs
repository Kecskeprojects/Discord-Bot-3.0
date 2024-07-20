using Discord_Bot.Resources;
using System.Collections.Generic;

namespace Discord_Bot.Services.Models.LastFm;
public class WhoKnows(List<UserResource> users)
{
    public List<UserResource> Users { get; set; } = users;
    public Dictionary<string, int> Plays { get; set; } = [];
    public string EmbedTitle { get; set; }
    public string ImageUrl { get; set; }
    public string Message { get; set; }
}
