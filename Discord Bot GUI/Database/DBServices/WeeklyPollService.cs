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
public class WeeklyPollService(
    IWeeklyPollRepository weeklyPollRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IWeeklyPollService
{
    private readonly IWeeklyPollRepository weeklyPollRepository = weeklyPollRepository;

    public async Task<List<WeeklyPollResource>> GetPollsByDayOfWeekAsync(DayOfWeek dayOfWeek)
    {
        List<WeeklyPollResource> result = null;
        try
        {
            List<WeeklyPoll> polls = await weeklyPollRepository.GetListAsync(
                wp => wp.RepeatOnDayOfWeek == dayOfWeek.ToString(),
                includes: [
                    wp => wp.Server,
                    wp => wp.Role,
                    wp => wp.Channel,
                    wp => wp.WeeklyPollOptions,
                    wp => wp.OptionPreset,
                    wp => wp.OptionPreset.WeeklyPollOptions
                ]);

            result = mapper.Map<List<WeeklyPoll>, List<WeeklyPollResource>>(polls);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollsByDayOfWeekAsync", ex);
        }
        return result;
    }
}
