using DatabaseORM.Enums;
using DatabaseORM.Resources;

namespace DatabaseORM.Interface.Service;

public interface IUserIdolService
{
    Task<DbProcessResultEnum> AddUserIdolAsync(ulong userId, string idolName, string idolGroup);
    Task<DbProcessResultEnum> AddUserIdolGroupAsync(ulong userId, string biasGroup);
    Task<DbProcessResultEnum> ClearUserIdolAsync(ulong userId);
    Task<List<IdolResource>> GetUserIdolsListAsync(ulong userId, string groupName);
    Task<DbProcessResultEnum> RemoveUserIdolAsync(ulong userId, string biasName, string idolGroup);
    Task<DbProcessResultEnum> RemoveUserIdolGroupAsync(ulong userId, string biasGroup);
}
