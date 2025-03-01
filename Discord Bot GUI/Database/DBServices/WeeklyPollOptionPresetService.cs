using AutoMapper;
using Discord_Bot.Communication.Modal;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Reflection;
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

    public async Task<WeeklyPollOptionPresetResource> GetOrCreateDummyPresetAsync()
    {
        WeeklyPollOptionPresetResource result = null;
        try
        {
            WeeklyPollOptionPreset preset = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
                wp => string.IsNullOrEmpty(wp.Name),
                wp => wp.WeeklyPollOptions);

            if (preset == null)
            {
                preset = new WeeklyPollOptionPreset()
                {
                    Name = string.Empty,
                    Description = string.Empty,
                    IsActive = false,
                    IsSpecialPreset = false,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow,
                };
                await weeklyPollOptionPresetRepository.AddAsync(preset);
            }

            result = mapper.Map<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>(preset);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetOrCreateDummyPresetAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollOptionPresetResource> GetPresetByIdAsync(int presetId)
    {
        WeeklyPollOptionPresetResource result = null;
        try
        {
            WeeklyPollOptionPreset preset = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
                wp => wp.WeeklyPollOptionPresetId == presetId,
                wp => wp.WeeklyPollOptions);

            result = mapper.Map<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>(preset);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetPresetByIdAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollOptionPresetResource> GetPresetByNameAsync(string presetName)
    {
        WeeklyPollOptionPresetResource result = null;
        try
        {
            presetName = presetName.ToLower();
            WeeklyPollOptionPreset poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
                wp => wp.Name.ToLower() == presetName,
                wp => wp.WeeklyPollOptions);

            result = mapper.Map<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetPresetByNameAsync", ex);
        }
        return result;
    }

    public async Task<List<WeeklyPollOptionPresetResource>> GetPresetsAsync()
    {
        List<WeeklyPollOptionPresetResource> result = null;
        try
        {
            List<WeeklyPollOptionPreset> presets = await weeklyPollOptionPresetRepository.GetAllAsync();
            if (presets == null)
            {
                return null;
            }

            result = mapper.Map<List<WeeklyPollOptionPreset>, List<WeeklyPollOptionPresetResource>>(presets);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetPresetsAsync", ex);
        }

        return result;
    }

    public async Task<DbProcessResultEnum> RemovePresetByNameAsync(string presetName)
    {
        try
        {
            presetName = presetName.ToLower();
            WeeklyPollOptionPreset poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
                p => p.Name.ToLower() == presetName,
                wp => wp.WeeklyPollOptions);
            if (poll != null)
            {
                await weeklyPollOptionPresetRepository.RemoveAsync(poll);

                logger.Log("Poll removed successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log("Poll not found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs RemovePresetByNameAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> UpdateAsync(int presetId, EditWeeklyPollOptionPresetModal modal)
    {
        try
        {
            WeeklyPollOptionPreset poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(p => p.WeeklyPollOptionPresetId == presetId);
            if (poll != null)
            {
                poll.Name = modal.Name;
                poll.Description = modal.Description;
                poll.ModifiedOn = DateTime.UtcNow;

                await weeklyPollOptionPresetRepository.SaveChangesAsync();
                logger.Log("Poll updated successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log("Poll not found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs UpdateAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> UpdateFieldAsync(int presetId, string fieldName, string newValue)
    {
        try
        {
            WeeklyPollOptionPreset poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(p => p.WeeklyPollOptionPresetId == presetId);

            PropertyInfo property = poll.GetType().GetProperty(fieldName);

            Enum.TryParse(property.PropertyType.Name, true, out TypeCode enumValue); //Get the type based on typecode

            object convertedValue = Convert.ChangeType(newValue, enumValue); //Convert the value to that of the typecode

            property.SetValue(poll, convertedValue); // Set the converted value

            poll.ModifiedOn = DateTime.UtcNow;
            await weeklyPollOptionPresetRepository.UpdateAsync(poll);
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs UpdateFieldAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
