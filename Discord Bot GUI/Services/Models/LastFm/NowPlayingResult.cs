using LastFmApi.Models.Recent;

namespace Discord_Bot.Services.Models.LastFm
{
    public class NowPlayingResult
    {
        public string ImageUrl { get; set; }
        public string Message { get; set; }
        public string TrackPlays { get; set; }
        public string Ranking { get; set; }
        public Attr Attr { get; set; }
        public string TrackName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string Url { get; set; }
        public string ArtistMbid { get; set; }
    }
}
