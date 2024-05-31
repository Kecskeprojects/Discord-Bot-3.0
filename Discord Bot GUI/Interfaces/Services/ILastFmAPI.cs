using Discord_Bot.Resources;
using Discord_Bot.Services.Models.LastFm;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services;

public interface ILastFmAPI
{
    Task<NowPlayingResult> GetNowPlayingAsync(string lastFmUsername);
    Task<LastFmListResult> GetRecentsAsync(string lastFmUsername, int limit);
    Task<LastFmListResult> GetTopAlbumsAsync(string lastFmUsername, int? limit, int? page, string period);
    Task<LastFmListResult> GetTopArtistsAsync(string lastFmUsername, int? limit, int? page, string period);
    Task<LastFmListResult> GetTopTracksAsync(string lastFmUsername, int? limit, int? page, string period);
    Task<ArtistStats> GetArtistDataAsync(string name, string artist_name);
    Task WhoKnowsByCurrentlyPlaying(WhoKnows wk, UserResource user);
    Task WhoKnowsByTrack(WhoKnows wk, string input);
    Task WhoKnowsByArtist(WhoKnows wk, string input);
}
