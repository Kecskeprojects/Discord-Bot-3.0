using Discord_Bot.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IIdolRepository
    {
        Task AddCustomCommandAsync(Idol idol);
        Task<List<Idol>> GetBiasesByGroupAsync(string groupName);
        Task<List<Idol>> GetBiasesByNamesAsync(string[] nameList);
        Task<List<Idol>> GetBiasesByNameAndGroupAsync(string biasName, string biasGroup);
        Task<List<Idol>> GetUserBiasesListAsync(ulong userId, string groupName);
        Task<bool> IdolExistsAsync(string biasName, string biasGroup);
        Task RemoveIdolAsync(Idol idol);
        Task<Idol> GetBiasByNameAndGroupAsync(string biasName, string biasGroup);
    }
}
