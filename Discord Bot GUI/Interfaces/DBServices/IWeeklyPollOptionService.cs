﻿using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IWeeklyPollOptionService
{
    Task<WeeklyPollOptionResource> GetOrCreateOptionAsync(bool isPresetOption, int Id, int optionId, byte orderNumber);
    Task<DbProcessResultEnum> UpdateAsync(int pollOptionId, string optionTitle);
}
