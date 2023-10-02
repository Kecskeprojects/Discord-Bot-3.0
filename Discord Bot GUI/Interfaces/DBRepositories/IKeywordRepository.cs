using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IKeywordRepository
    {
        Task AddCustomCommandAsync(Keyword keyword);
        Task<Keyword> GetKeywordAsync(ulong serverId, string trigger);
        Task<bool> KeywordExistsAsync(ulong serverId, string trigger);
        Task RemoveKeywordAsync(Keyword keyword);
    }
}
