using Discord_Bot.Database.Models;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IKeywordRepository
    {
        Task<Keyword> GetRoleAsync(ulong serverId, string trigger);
    }
}
