using Discord_Bot.Enums;
using Discord_Bot.Services.Models.SpotifyAPI;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services;

public interface ISpotifyAPI
{
    Task<SearchResultEnum> SpotifySearch(string query, ulong serverId, ulong channelId, string username);
    //Task<string> ImageSearch(string artist, string song = "", string[] tags = null);
    Task<SpotifyImageSearchResult> SearchItemAsync(string mbid, string artistName, string songName = "");
}
