using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices
{
    public interface IBirthdayService
    {
        Task<DbProcessResultEnum> AddBirthdayAsync(ulong serverId, ulong userId, DateTime date);
        Task<List<BirthdayResource>> GetBirthdaysByDateAsync();
        Task<List<BirthdayResource>> GetServerBirthdayListAsync(ulong serverId);
        Task<DbProcessResultEnum> RemoveBirthdayAsync(ulong serverId, ulong userId);
    }
}
