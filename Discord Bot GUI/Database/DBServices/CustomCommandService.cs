using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Database.Models;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices;

public class CustomCommandService(
    ICustomCommandRepository customCommandRepository,
    IServerRepository serverRepository,
    IMapper mapper,
    Logging logger,
    Cache cache) : BaseService(mapper, logger, cache), ICustomCommandService
{
    private readonly ICustomCommandRepository customCommandRepository = customCommandRepository;
    private readonly IServerRepository serverRepository = serverRepository;

    public async Task<DbProcessResultEnum> AddCustomCommandAsync(ulong serverId, string commandName, string link)
    {
        try
        {
            commandName = commandName.Trim().ToLower();
            if (await customCommandRepository.ExistsAsync(
                cc => cc.Server.DiscordId == serverId.ToString()
                && cc.Command.Trim().ToLower().Equals(commandName),
                cc => cc.Server))
            {
                return DbProcessResultEnum.AlreadyExists;
            }

            Server server = await serverRepository.FirstOrDefaultAsync(s => s.DiscordId == serverId.ToString());

            CustomCommand command = new()
            {
                CommandId = 0,
                Server = server,
                Command = commandName,
                Url = link

            };
            await customCommandRepository.AddAsync(command);

            logger.Log("Custom command added successfully!");
            return DbProcessResultEnum.Success;
        }
        catch (Exception ex)
        {
            logger.Error("CustomCommandService.cs AddCustomCommandAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }

    public async Task<CustomCommandResource> GetCustomCommandAsync(ulong serverId, string commandName)
    {
        CustomCommandResource result = null;
        try
        {
            commandName = commandName.Trim().ToLower();
            CustomCommand command = await customCommandRepository.FirstOrDefaultAsync(
                cc => cc.Server.DiscordId == serverId.ToString()
                && cc.Command.Trim().ToLower().Equals(commandName),
                cc => cc.Server);
            result = mapper.Map<CustomCommand, CustomCommandResource>(command);
        }
        catch (Exception ex)
        {
            logger.Error("CustomCommandService.cs GetCustomCommandAsync", ex);
        }
        return result;
    }

    public async Task<List<CustomCommandResource>> GetServerCustomCommandListAsync(ulong serverId)
    {
        List<CustomCommandResource> result = null;
        try
        {
            List<CustomCommand> commands = await customCommandRepository.GetListAsync(
                cc => cc.Server.DiscordId == serverId.ToString(),
                includes: cc => cc.Server,
                orderBy: cc => cc.Command,
                ascending: true);
            result = mapper.Map<List<CustomCommand>, List<CustomCommandResource>>(commands);
        }
        catch (Exception ex)
        {
            logger.Error("CustomCommandService.cs GetServerCustomCommandListAsync", ex);
        }
        return result;
    }

    public async Task<DbProcessResultEnum> RemoveCustomCommandAsync(ulong serverId, string commandName)
    {
        try
        {
            commandName = commandName.Trim().ToLower();
            CustomCommand customCommand = await customCommandRepository.FirstOrDefaultAsync(
                cc => cc.Server.DiscordId == serverId.ToString()
                && cc.Command.Trim().ToLower().Equals(commandName),
                cc => cc.Server);
            if (customCommand != null)
            {
                await customCommandRepository.RemoveAsync(customCommand);

                logger.Log($"Custom command {commandName} removed successfully!");
                return DbProcessResultEnum.Success;
            }
            else
            {
                logger.Log($"Custom command {commandName} could not be found!");
                return DbProcessResultEnum.NotFound;
            }
        }
        catch (Exception ex)
        {
            logger.Error("CustomCommandService.cs RemoveCustomCommandAsync", ex);
        }
        return DbProcessResultEnum.Failure;
    }
}
