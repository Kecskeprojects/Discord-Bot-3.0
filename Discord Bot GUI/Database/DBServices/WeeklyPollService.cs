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
public class WeeklyPollService(
    IWeeklyPollRepository weeklyPollRepository,
    IServerRepository serverRepository,
    IChannelRepository channelRepository,
    IRoleRepository roleRepository,
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IWeeklyPollService
{
    private readonly IWeeklyPollRepository weeklyPollRepository = weeklyPollRepository;

    public async Task<WeeklyPollEditResource> GetOrCreateDummyPollAsync(ulong serverId)
    {
        WeeklyPollEditResource result = null;
        try
        {
            WeeklyPoll poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.Server.DiscordId == serverId.ToString() && string.IsNullOrEmpty(wp.Name),
                wp => wp.WeeklyPollOptions);

            if (poll == null)
            {
                Server server = await serverRepository.FirstOrDefaultAsync(x => x.DiscordId == serverId.ToString());
                server ??= new Server() { DiscordId = serverId.ToString() };

                poll = new WeeklyPoll()
                {
                    Name = string.Empty,
                    Title = string.Empty,
                    CloseInTimeSpanTicks = long.MinValue,
                    IsActive = false,
                    IsMultipleAnswer = false,
                    RepeatOnDayOfWeek = Enum.GetName(DayOfWeek.Monday),
                    ChannelId = null,
                    OptionPresetId = null,
                    RoleId = null,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow,
                    Server = server,
                };
                await weeklyPollRepository.AddAsync(poll);
            }

            result = mapper.Map<WeeklyPoll, WeeklyPollEditResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetOrCreateDummyPollAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollResource> GetPollByIdAsync(int pollId)
    {
        WeeklyPollResource result = null;
        try
        {
            WeeklyPoll poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.WeeklyPollId == pollId,
                includes: [wp => wp.Role,
                wp => wp.Channel,
                wp => wp.WeeklyPollOptions]);

            result = mapper.Map<WeeklyPoll, WeeklyPollResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollByIdAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollEditResource> GetPollByIdForEditAsync(int pollId)
    {
        WeeklyPollEditResource result = null;
        try
        {
            WeeklyPoll poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.WeeklyPollId == pollId,
                wp => wp.WeeklyPollOptions);

            result = mapper.Map<WeeklyPoll, WeeklyPollEditResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollByIdForEditAsync", ex);
        }
        return result;
    }

    public async Task<WeeklyPollEditResource> GetPollByNameForEditAsync(ulong serverId, string pollName)
    {
        WeeklyPollEditResource result = null;
        try
        {
            pollName = pollName.ToLower();
            WeeklyPoll poll = await weeklyPollRepository.FirstOrDefaultAsync(
                wp => wp.Server.DiscordId == serverId.ToString() && wp.Name.ToLower() == pollName,
                wp => wp.WeeklyPollOptions);

            result = mapper.Map<WeeklyPoll, WeeklyPollEditResource>(poll);
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs GetPollByNameForEditAsync", ex);
        }
        return result;
    }

    public async Task<List<WeeklyPollResource>> GetPollsByDayOfWeekAsync(DayOfWeek dayOfWeek)
    {
        List<WeeklyPollResource> result = null;
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

    public async Task<List<WeeklyPollResource>> GetPollsByServerIdAsync(ulong serverId)
    {
        List<WeeklyPollResource> result = null;
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
                    wp => wp.OptionPreset.WeeklyPollOptions
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
            WeeklyPoll poll = await weeklyPollRepository.FirstOrDefaultAsync(p => p.Server.DiscordId == serverId.ToString() && p.Name.ToLower() == pollName);
            if (poll != null)
            {
                await weeklyPollRepository.RemoveAsync(poll);

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

    public async Task<DbProcessResultEnum> UpdateAsync(int pollId, EditWeeklyPollModal modal, ulong channelId, ulong? roleId)
    {
        try
        {
            WeeklyPoll poll = await weeklyPollRepository.FirstOrDefaultAsync(p => p.WeeklyPollId == pollId);
            if (poll != null)
            {
                Channel channel = await channelRepository.FirstOrDefaultAsync(c => c.DiscordId == channelId.ToString());
                channel ??= new Channel()
                {
                    DiscordId = channelId.ToString(),
                    ServerId = poll.ServerId
                };

                Role role = await roleRepository.FirstOrDefaultAsync(r => r.DiscordId == roleId.ToString());
                if (roleId.HasValue)
                {
                    role ??= new Role()
                    {
                        DiscordId = roleId.ToString(),
                        ServerId = poll.ServerId
                    };
                }
                else
                {
                    poll.RoleId = null;
                }

                poll.Name = modal.Name;
                poll.Title = modal.PollTitle;
                poll.Channel = channel;
                poll.Role = role;
                poll.ModifiedOn = DateTime.UtcNow;

                await weeklyPollRepository.SaveChangesAsync();
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
            if(newValue == "custom")
            {
                newValue = null;
            }

            WeeklyPoll poll = await weeklyPollRepository.FirstOrDefaultAsync(p => p.WeeklyPollId == pollId);

            PropertyInfo property = poll.GetType().GetProperty(fieldName);

            //Get the type we need to cast to.
            object convertedValue;
            if (property.PropertyType == typeof(Nullable<Int32>))
            {
                convertedValue = int.TryParse(newValue, out int tempValue) ? tempValue : null;
            }
            else
            {
                Enum.TryParse(property.PropertyType.Name, true, out TypeCode enumValue); //Get the type based on typecode

                convertedValue = Convert.ChangeType(newValue, enumValue); //Convert the value to that of the typecode
            }

            property.SetValue(poll, convertedValue); // Set the converted value

            poll.ModifiedOn = DateTime.UtcNow;
            await weeklyPollRepository.UpdateAsync(poll);
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("WeeklyPollService.cs UpdateFieldAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
