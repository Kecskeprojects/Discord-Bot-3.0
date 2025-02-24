using Discord_Bot.Communication.Modal;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IWeeklyPollService
{
    Task<WeeklyPollEditResource> GetOrCreateDummyPollAsync(ulong serverId);
    Task<WeeklyPollResource> GetPollByIdAsync(int pollId);
    Task<WeeklyPollEditResource> GetPollByNameForEditAsync(ulong serverId, string pollName);
    Task<WeeklyPollEditResource> GetPollByIdForEditAsync(int pollId);
    Task<List<WeeklyPollResource>> GetPollsByDayOfWeekAsync(DayOfWeek dayOfWeek);
    Task<List<WeeklyPollResource>> GetPollsByServerIdAsync(ulong serverId);
    Task<DbProcessResultEnum> RemovePollByNameAsync(ulong serverId, string pollName);
    Task<DbProcessResultEnum> UpdateAsync(int pollId, EditWeeklyPollModal modal, ulong channelId, ulong? roleId, string roleName);
    Task<DbProcessResultEnum> UpdateFieldAsync(int pollId, string fieldName, string newValue);
}
