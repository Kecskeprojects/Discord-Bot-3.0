using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IIdolRepository
    {
        Task AddIdolAsync(Idol idol);
        Task<List<Idol>> GetIdolsByGroupAsync(string groupName);
        Task<List<Idol>> GetIdolsByNamesAsync(string[] nameList);
        Task<List<Idol>> GetIdolsByNameAndGroupAsync(string idolName, string idolGroup);
        Task<List<Idol>> GetUserIdolsListAsync(ulong userId, string groupName);
        Task<bool> IdolExistsAsync(string idolName, string idolGroup);
        Task RemoveIdolAsync(Idol idol);
        Task<Idol> GetIdolByNameAndGroupAsync(string idolName, string idolGroup);
        Task<Idol> GetIdolWithAliasesAsync(string idolName, string idolGroup);
        Task UpdateIdolAsync(Idol idol);
    }
}
