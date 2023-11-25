using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IIdolService
    {
        Task<DbProcessResultEnum> AddBiasAsync(string biasName, string biasGroup);
        Task<DbProcessResultEnum> AddUserBiasAsync(ulong userId, string biasName, string biasGroup);
        Task<DbProcessResultEnum> ClearUserBiasAsync(ulong userId);
        Task<List<IdolResource>> GetBiasesByGroupAsync(string groupName);
        Task<ListWithDbResult<UserResource>> GetUsersWithBiasesAsync(string[] nameList);
        Task<DbProcessResultEnum> RemoveBiasAsync(string biasName, string biasGroup);
        Task<List<IdolResource>> GetUserBiasesListAsync(ulong userId, string groupName);
        Task<DbProcessResultEnum> RemoveUserBiasAsync(ulong userId, string biasName, string biasGroup);
    }
}
