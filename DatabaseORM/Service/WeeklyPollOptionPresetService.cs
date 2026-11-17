using DatabaseORM.Caching;
using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Repository;
using DatabaseORM.Interface.Service;
using DatabaseORM.Model;
using DatabaseORM.Resources;
using MapsterMapper;
using System.Globalization;
using System.Reflection;

namespace DatabaseORM.Service;

public class WeeklyPollOptionPresetService(
    IWeeklyPollOptionPresetRepository weeklyPollOptionPresetRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IWeeklyPollOptionPresetService
{
    private readonly IWeeklyPollOptionPresetRepository weeklyPollOptionPresetRepository = weeklyPollOptionPresetRepository;

    public async Task<List<WeeklyPollOptionPresetResource>> GetActivePresetsAsync()
    {
        List<WeeklyPollOptionPresetResource> result = [];
        try
        {
            List<WeeklyPollOptionPreset> presets = await weeklyPollOptionPresetRepository.GetListAsync(p => p.IsActive);
            if (presets == null)
            {
                return [];
            }

            result = mapper.Map<List<WeeklyPollOptionPreset>, List<WeeklyPollOptionPresetResource>>(presets);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetActivePresetsAsync", ex);
        }

        return result;
    }

    public async Task<WeeklyPollOptionPresetResource?> GetOrCreateDummyPresetAsync()
    {
        WeeklyPollOptionPresetResource? result = null;
        try
        {
            WeeklyPollOptionPreset? preset = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
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
                _ = await weeklyPollOptionPresetRepository.AddAsync(preset);
            }

            result = mapper.Map<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>(preset);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetOrCreateDummyPresetAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollOptionPresetResource?> GetPresetByIdAsync(int presetId)
    {
        WeeklyPollOptionPresetResource? result = null;
        try
        {
            WeeklyPollOptionPreset? preset = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
                wp => wp.WeeklyPollOptionPresetId == presetId,
                wp => wp.WeeklyPollOptions);

            result = preset == null ? null : mapper.Map<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>(preset);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetPresetByIdAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollOptionPresetResource?> GetPresetByNameAsync(string presetName)
    {
        WeeklyPollOptionPresetResource? result = null;
        try
        {
            presetName = presetName.ToLower();
            WeeklyPollOptionPreset? poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
                wp => wp.Name.ToLower() == presetName,
                wp => wp.WeeklyPollOptions);

            result = poll == null ? null : mapper.Map<WeeklyPollOptionPreset, WeeklyPollOptionPresetResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs GetPresetByNameAsync", ex);
        }
        return result;
    }

    public async Task<List<WeeklyPollOptionPresetResource>> GetPresetsAsync()
    {
        List<WeeklyPollOptionPresetResource> result = [];
        try
        {
            List<WeeklyPollOptionPreset> presets = await weeklyPollOptionPresetRepository.GetAllAsync();
            if (presets == null)
            {
                return [];
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
            WeeklyPollOptionPreset? poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(
                p => p.Name.ToLower() == presetName,
                wp => wp.WeeklyPollOptions);
            if (poll != null)
            {
                _ = await weeklyPollOptionPresetRepository.RemoveAsync(poll);

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

    public async Task<DbProcessResultEnum> UpdateAsync(int presetId, string name, string description)
    {
        try
        {
            WeeklyPollOptionPreset? poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(p => p.WeeklyPollOptionPresetId == presetId);
            if (poll != null)
            {
                poll.Name = name;
                poll.Description = description;
                poll.ModifiedOn = DateTime.UtcNow;

                _ = await weeklyPollOptionPresetRepository.SaveChangesAsync();
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
            WeeklyPollOptionPreset? poll = await weeklyPollOptionPresetRepository.FirstOrDefaultAsync(p => p.WeeklyPollOptionPresetId == presetId);
            if (poll == null)
            {
                return DbProcessResultEnum.NotFound;
            }

            PropertyInfo property = poll.GetType().GetProperty(fieldName)
                ?? throw new InvalidOperationException($"WeeklyPollOptionPreset has no property {fieldName}");

            _ = Enum.TryParse(property.PropertyType.Name, true, out TypeCode enumValue); //Get the type based on typecode

            object? convertedValue = Convert.ChangeType(newValue, enumValue, CultureInfo.InvariantCulture); //Convert the value to that of the typecode

            property.SetValue(poll, convertedValue); // Set the converted value

            poll.ModifiedOn = DateTime.UtcNow;
            _ = await weeklyPollOptionPresetRepository.UpdateAsync(poll);
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollOptionPresetService.cs UpdateFieldAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
