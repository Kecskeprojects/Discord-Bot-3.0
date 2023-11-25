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
        Task<DbProcessResultEnum> AddUserBiasAsync(ulong id, string biasName, string biasGroup);
        Task<DbProcessResultEnum> ClearUserBiasAsync(ulong id);
        Task<List<IdolResource>> GetBiasesByGroupAsync(string groupName);
        Task<List<IdolResource>> GetUserBiasesByGroupAsync(string groupName, ulong userId);
        ListWithDbResult<UserResource> GetUsersWithBiasesAsync(string[] nameList);
        Task<DbProcessResultEnum> RemoveBiasAsync(string biasName, string biasGroup);
        List<IdolResource> UserBiasesListAsync(ulong userId, string groupName);
    }
}
