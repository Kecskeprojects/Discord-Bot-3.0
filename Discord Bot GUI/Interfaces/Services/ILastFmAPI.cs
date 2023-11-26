using Discord_Bot.Services.Models.LastFm;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface ILastFmAPI
    {
        Task<NowPlayingResult> GetNowPlayingAsync(string lastFmUsername);
        Task<LastFmListResult> GetTopAlbumsAsync(string lastFmUsername, int? limit, int? page, string period);
        Task<LastFmListResult> GetTopArtistsAsync(string lastFmUsername, int? limit, int? page, string period);
        Task<LastFmListResult> GetTopTracksAsync(string lastFmUsername, int? limit, int? page, string period);
    }
}
