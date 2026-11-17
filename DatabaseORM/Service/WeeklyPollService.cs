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

public class WeeklyPollService(
    IWeeklyPollRepository weeklyPollRepository,
    IServerRepository serverRepository,
    IChannelRepository channelRepository,
    IRoleRepository roleRepository,
    IMapper mapper,
    IBotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IWeeklyPollService
{
    private readonly IWeeklyPollRepository weeklyPollRepository = weeklyPollRepository;

    public async Task<WeeklyPollEditResource?> GetOrCreateDummyPollAsync(ulong serverId)
    {
        WeeklyPollEditResource? result = null;
        try
        {
            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.Server.DiscordId == serverId.ToString() && string.IsNullOrEmpty(wp.Name),
                wp => wp.WeeklyPollOptions);

            if (poll == null)
            {
                Server? server = await serverRepository.FirstOrDefaultAsync(x => x.DiscordId == serverId.ToString());
                server ??= new Server() { DiscordId = serverId.ToString() };

                poll = new WeeklyPoll()
                {
                    Name = string.Empty,
                    Title = string.Empty,
                    CloseInTimeSpanTicks = long.MinValue,
                    IsActive = false,
                    IsMultipleAnswer = false,
                    RepeatOnDayOfWeek = nameof(DayOfWeek.Monday),
                    ChannelId = null,
                    OptionPresetId = null,
                    RoleId = null,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow,
                    Server = server,
                };
                _ = await weeklyPollRepository.AddAsync(poll);
            }

            result = mapper.Map<WeeklyPoll, WeeklyPollEditResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetOrCreateDummyPollAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollResource?> GetPollByIdAsync(int pollId)
    {
        WeeklyPollResource? result = null;
        try
        {
            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.WeeklyPollId == pollId,
                includes: [wp => wp.Role,
                wp => wp.Channel,
                wp => wp.WeeklyPollOptions]);

            result = poll == null ? null : mapper.Map<WeeklyPoll, WeeklyPollResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollByIdAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollEditResource?> GetPollByIdForEditAsync(int pollId)
    {
        WeeklyPollEditResource? result = null;
        try
        {
            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.WeeklyPollId == pollId,
                wp => wp.WeeklyPollOptions);

            result = poll == null ? null : mapper.Map<WeeklyPoll, WeeklyPollEditResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollByIdForEditAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollEditResource?> GetPollByNameForEditAsync(ulong serverId, string pollName)
    {
        WeeklyPollEditResource? result = null;
        try
        {
            pollName = pollName.ToLower();
            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.Server.DiscordId == serverId.ToString() && wp.Name.ToLower() == pollName,
                wp => wp.WeeklyPollOptions);

            result = poll == null ? null : mapper.Map<WeeklyPoll, WeeklyPollEditResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollByNameForEditAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollResource?> GetPollForForceResendAsync(ulong id, string pollName)
    {
        WeeklyPollResource? result = null;
        try
        {
            pollName = pollName.ToLower();
            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.Server.DiscordId == id.ToString() && wp.Name.ToLower() == pollName,
                includes: [
                    wp => wp.Server,
                    wp => wp.Role,
                    wp => wp.Channel,
                    wp => wp.WeeklyPollOptions,
                    wp => wp.OptionPreset,
                    wp => wp.OptionPreset!.WeeklyPollOptions
                ]);

            result = poll == null ? null : mapper.Map<WeeklyPoll, WeeklyPollResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollForForceResendAsync", ex);
        }
        return result;
    }

    public async Task<List<WeeklyPollResource>> GetPollsByDayOfWeekAsync(DayOfWeek dayOfWeek)
    {
        List<WeeklyPollResource> result = [];
        try
        {
            List<WeeklyPoll> polls = await weeklyPollRepository.GetListAsync(
                wp => wp.RepeatOnDayOfWeek == dayOfWeek.ToString() && wp.IsActive,
                includes: [
                    wp => wp.Server,
                    wp => wp.Role,
                    wp => wp.Channel,
                    wp => wp.WeeklyPollOptions,
                    wp => wp.OptionPreset,
                    wp => wp.OptionPreset!.WeeklyPollOptions
                ]);

            result = mapper.Map<List<WeeklyPoll>, List<WeeklyPollResource>>(polls);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollsByDayOfWeekAsync", ex);
        }
        return result;
    }

    public async Task<List<WeeklyPollResource>> GetPollsByServerIdAsync(ulong serverId)
    {
        List<WeeklyPollResource> result = [];
        try
        {
            List<WeeklyPoll> polls = await weeklyPollRepository.GetListAsync(
                wp => wp.Server.DiscordId == serverId.ToString() && !string.IsNullOrEmpty(wp.Name),
                includes: [
                    wp => wp.Server,
                    wp => wp.Role,
                    wp => wp.Channel,
                    wp => wp.WeeklyPollOptions,
                    wp => wp.OptionPreset,
                    wp => wp.OptionPreset!.WeeklyPollOptions
                ]);

            result = mapper.Map<List<WeeklyPoll>, List<WeeklyPollResource>>(polls);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollsByServerIdAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> RemovePollByNameAsync(ulong serverId, string pollName)
    {
        try
        {
            pollName = pollName.ToLower();
            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(p => p.Server.DiscordId == serverId.ToString() && p.Name.ToLower() == pollName);
            if (poll != null)
            {
                _ = await weeklyPollRepository.RemoveAsync(poll);

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
            logger.Error("WeeklyPollService.cs RemovePollByNameAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> UpdateAsync(int pollId, string name, string title, ulong channelId, RoleResource? role)
    {
        try
        {
            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(p => p.WeeklyPollId == pollId);
            if (poll != null)
            {
                Channel? channel = await channelRepository.FirstOrDefaultAsync(c => c.DiscordId == channelId.ToString());
                channel ??= new Channel()
                {
                    DiscordId = channelId.ToString(),
                    ServerId = poll.ServerId
                };

                Role? pollRole = null;
                if (role != null)
                {
                    string roleDiscordId = role.DiscordId.ToString();
                    pollRole = await roleRepository.FirstOrDefaultAsync(r => r.DiscordId == roleDiscordId);
                    pollRole ??= new Role()
                    {
                        DiscordId = roleDiscordId,
                        RoleName = role.RoleName,
                        ServerId = poll.ServerId
                    };
                }
                else
                {
                    poll.RoleId = null;
                }

                poll.Name = name;
                poll.Title = title;
                poll.Channel = channel;
                poll.Role = pollRole;
                poll.ModifiedOn = DateTime.UtcNow;

                _ = await weeklyPollRepository.SaveChangesAsync();
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
            logger.Error("WeeklyPollService.cs UpdateAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<DbProcessResultEnum> UpdateFieldAsync(int pollId, string fieldName, string newValue)
    {
        try
        {
            //"custom" in the preset select means no preset (custom answers), stored as a null OptionPresetId
            string? value = newValue == "custom" ? null : newValue;

            WeeklyPoll? poll = await weeklyPollRepository.FirstOrDefaultAsync(p => p.WeeklyPollId == pollId);
            if (poll == null)
            {
                return DbProcessResultEnum.NotFound;
            }

            PropertyInfo property = poll.GetType().GetProperty(fieldName)
                ?? throw new InvalidOperationException($"WeeklyPoll has no property {fieldName}");

            //Get the type we need to cast to.
            object? convertedValue;
            if (property.PropertyType == typeof(int?))
            {
                convertedValue = int.TryParse(value, out int tempValue) ? tempValue : null;
            }
            else
            {
                _ = Enum.TryParse(property.PropertyType.Name, true, out TypeCode enumValue); //Get the type based on typecode

                convertedValue = Convert.ChangeType(value, enumValue, CultureInfo.InvariantCulture); //Convert the value to that of the typecode
            }

            property.SetValue(poll, convertedValue); // Set the converted value

            poll.ModifiedOn = DateTime.UtcNow;
            _ = await weeklyPollRepository.UpdateAsync(poll);
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs UpdateFieldAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
