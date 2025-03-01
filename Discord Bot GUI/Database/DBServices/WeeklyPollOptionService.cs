using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;
public class WeeklyPollOptionService(
    IWeeklyPollOptionRepository weeklyPollOptionRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IWeeklyPollOptionService
{
    private readonly IWeeklyPollOptionRepository weeklyPollOptionRepository = weeklyPollOptionRepository;

    public async Task<WeeklyPollOptionResource> GetOrCreateOptionAsync(bool isPresetOption, int foreignId, int optionId, byte orderNumber)
    {
        WeeklyPollOptionResource result = null;
        try
        {
            WeeklyPollOption pollOption;
            if (optionId == 0)
            {
                pollOption = new()
                {
                    OrderNumber = orderNumber,
                    WeeklyPollId = isPresetOption ? null : foreignId,
                    WeeklyPollOptionPresetId = isPresetOption ? foreignId : null,
                    Title = string.Empty,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow,
                };
                await weeklyPollOptionRepository.AddAsync(pollOption);
            }
            else
            {
                pollOption = await weeklyPollOptionRepository.FirstOrDefaultAsync(wp => wp.WeeklyPollOptionId == optionId);
            }

            result = mapper.Map<WeeklyPollOption, WeeklyPollOptionResource>(pollOption);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionService.cs GetOrCreateOptionAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> UpdateAsync(int pollOptionId, string optionTitle)
    {
        try
        {
            WeeklyPollOption pollOption = await weeklyPollOptionRepository.FirstOrDefaultAsync(p => p.WeeklyPollOptionId == pollOptionId);
            if (pollOption != null)
            {
                pollOption.Title = optionTitle;
                pollOption.ModifiedOn = DateTime.UtcNow;

                await weeklyPollOptionRepository.SaveChangesAsync();
                logger.Log("Poll Option updated successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log("Poll Option not found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs RemovePollByNameAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
