using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface ISpotifyAPI
    {
        Task<SearchResultEnum> SpotifySearch(string query, ulong serverId, ulong channelId, string username);
        Task<string> ImageSearch(string artist, string song = "", string[] tags = null);
    }
}
