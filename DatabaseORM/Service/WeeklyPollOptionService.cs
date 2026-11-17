using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using MapsterMapper;

namespace DatabaseORM.Service;

public class WeeklyPollOptionService(
    IWeeklyPollOptionRepository weeklyPollOptionRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IWeeklyPollOptionService
{
    private readonly IWeeklyPollOptionRepository weeklyPollOptionRepository = weeklyPollOptionRepository;

    public async Task<WeeklyPollOptionResource?> GetOrCreateOptionAsync(bool isPresetOption, int Id, int optionId, byte orderNumber)
    {
        WeeklyPollOptionResource? result = null;
        try
        {
            WeeklyPollOption? pollOption;
            if (optionId == 0)
            {
                pollOption = new()
                {
                    OrderNumber = orderNumber,
                    WeeklyPollId = isPresetOption ? null : Id,
                    WeeklyPollOptionPresetId = isPresetOption ? Id : null,
                    Title = string.Empty,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow,
                };
                _ = await weeklyPollOptionRepository.AddAsync(pollOption);
            }
            else
            {
                pollOption = await weeklyPollOptionRepository.FirstOrDefaultAsync(wp => wp.WeeklyPollOptionId == optionId);
            }

            result = pollOption == null ? null : mapper.Map<WeeklyPollOption, WeeklyPollOptionResource>(pollOption);
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
            WeeklyPollOption? pollOption = await weeklyPollOptionRepository.FirstOrDefaultAsync(p => p.WeeklyPollOptionId == pollOptionId);
            if (pollOption != null)
            {
                pollOption.Title = optionTitle;
                pollOption.ModifiedOn = DateTime.UtcNow;

                _ = await weeklyPollOptionRepository.SaveChangesAsync();
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
