namespace Discord_Bot.Services.Models.LastFm
{
    public class LastFmListResult
    {
        public string Message { get; internal set; }
        public string ImageUrl { get; internal set; }
        public int TotalPlays { get; internal set; }
        public string[] EmbedFields { get; internal set; } = ["", "", ""];
    }
}
