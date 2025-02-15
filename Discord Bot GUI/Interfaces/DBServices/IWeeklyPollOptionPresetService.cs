﻿using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBServices;
public interface IWeeklyPollOptionPresetService
{
    Task<List<WeeklyPollOptionPresetResource>> GetActivePresetsAsync();
}
