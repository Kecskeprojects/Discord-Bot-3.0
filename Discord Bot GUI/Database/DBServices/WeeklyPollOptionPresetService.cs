using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;
public class WeeklyPollOptionPresetService(
    IWeeklyPollOptionPresetRepository weeklyPollOptionPresetRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IWeeklyPollOptionPresetService
{
    private readonly IWeeklyPollOptionPresetRepository weeklyPollOptionPresetRepository = weeklyPollOptionPresetRepository;

    public async Task<List<WeeklyPollOptionPresetResource>> GetActivePresetsAsync()
    {
        List<WeeklyPollOptionPresetResource> result = null;
        try
        {
            List<WeeklyPollOptionPreset> presets = await weeklyPollOptionPresetRepository.GetListAsync(p => p.IsActive);
            if (presets == null)
            {
                return null;
            }

            result = mapper.Map<List<WeeklyPollOptionPreset>, List<WeeklyPollOptionPresetResource>>(presets);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetActivePresetsAsync", ex);
        }

        return result;
    }
}
