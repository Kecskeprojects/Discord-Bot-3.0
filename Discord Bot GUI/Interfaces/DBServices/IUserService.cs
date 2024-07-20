using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IUserService
{
    Task<DbProcessResultEnum> AddLastfmUsernameAsync(ulong userId, string name);
    Task<List<UserResource>> GetAllLastFmUsersAsync();
    Task<UserBiasGameStatResource> GetTopIdolsAsync(ulong userId, GenderEnum gender);
    Task<UserResource> GetUserAsync(ulong userId);
    Task<DbProcessResultEnum> RemoveLastfmUsernameAsync(ulong userId);
}
