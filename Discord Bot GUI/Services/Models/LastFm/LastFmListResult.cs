namespace Discord_Bot.Services.Models.LastFm;

public class LastFmListResult
{
    public string Message { get; set; }
    public string ImageUrl { get; set; }
    public int TotalPlays { get; set; }
    public string[] EmbedFields { get; set; } = ["", "", ""];
}
