using LastFmApi.Models.Recent;

namespace Discord_Bot.Services.Models.LastFm
{
    public class NowPlayingResult
    {
        public string ImageUrl { get; internal set; }
        public string Message { get; internal set; }
        public string TrackPlays { get; internal set; }
        public string Ranking { get; internal set; }
        public Attr Attr { get; internal set; }
        public string TrackName { get; internal set; }
        public string ArtistName { get; internal set; }
        public string AlbumName { get; internal set; }
        public string Url { get; internal set; }
    }
}
