using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;

public interface IServerMutedUserService
{
    Task<DbProcessResultEnum> AddMutedUserAsync(ulong serverId, ulong userId, DateTime mutedUntil, string removedRoles);
    Task<List<ServerMutedUserResource>> GetExpiredMutedUsersAsync(DateTime dateTime);
    Task<ServerMutedUserResource> GetMutedUserByUsernameAsync(ulong serverId, ulong userId);
    Task<DbProcessResultEnum> RemoveMutedUserAsync(ulong serverId, ulong userId);
}
