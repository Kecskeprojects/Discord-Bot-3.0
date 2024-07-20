using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IUserIdolService
{
    Task<DbProcessResultEnum> AddUserIdolAsync(ulong userId, string idolName, string idolGroup);
    Task<DbProcessResultEnum> AddUserIdolGroupAsync(ulong userId, string biasGroup);
    Task<DbProcessResultEnum> ClearUserIdolAsync(ulong userId);
    Task<List<IdolResource>> GetUserIdolsListAsync(ulong userId, string groupName);
    Task<DbProcessResultEnum> RemoveUserIdolAsync(ulong userId, string biasName, string idolGroup);
    Task<DbProcessResultEnum> RemoveUserIdolGroupAsync(ulong userId, string biasGroup);
}
