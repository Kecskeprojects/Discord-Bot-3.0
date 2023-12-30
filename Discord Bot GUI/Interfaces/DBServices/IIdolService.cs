using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IIdolService
    {
        Task<DbProcessResultEnum> AddIdolAsync(string idolName, string idolGroup);
        Task<DbProcessResultEnum> AddUserIdolAsync(ulong userId, string idolName, string idolGroup);
        Task<DbProcessResultEnum> ClearUserIdolAsync(ulong userId);
        Task<List<IdolResource>> GetIdolsByGroupAsync(string groupName);
        Task<ListWithDbResult<UserResource>> GetUsersWithIdolsAsync(string[] nameList);
        Task<DbProcessResultEnum> RemoveIdolAsync(string idolName, string idolGroup);
        Task<List<IdolResource>> GetUserIdolsListAsync(ulong userId, string groupName);
        Task<DbProcessResultEnum> RemoveUserIdolAsync(ulong userId, string idolName, string idolGroup);
        Task<List<IdolResource>> GetAllIdolsAsync();
        Task UpdateIdolDetailsAsync(IdolResource idolResource, ExtendedBiasData data, AdditionalIdolData additional);
    }
}
