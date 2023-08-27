using Discord.Commands;
using Discord_Bot.Enums;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.Services
{
    public interface IYoutubeAPI
    {
        Task<SearchResultEnum> Searching(string query, string username);
    }
}
