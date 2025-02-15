using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IWeeklyPollService
{
    Task<WeeklyPollEditResource> GetOrCreateDummyPollAsync(ulong serverId);
    Task<WeeklyPollEditResource> GetPollByNameForEditAsync(ulong serverId, string pollName);
    Task<List<WeeklyPollResource>> GetPollsByDayOfWeekAsync(DayOfWeek dayOfWeek);
    Task<List<WeeklyPollResource>> GetPollsByServerIdAsync(ulong serverId);
    Task<DbProcessResultEnum> RemovePollByNameAsync(ulong serverId, string pollName);
}
