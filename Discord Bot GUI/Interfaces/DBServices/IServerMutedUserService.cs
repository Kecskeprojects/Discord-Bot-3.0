using Discord_Bot.Enums;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IServerMutedUserService
{
    Task<DbProcessResultEnum> AddMutedUserAsync(ulong serverId, ulong userId, DateTime mutedUntil);
    Task<DbProcessResultEnum> RemoveMutedUserAsync(ulong serverId, ulong userId);
}
