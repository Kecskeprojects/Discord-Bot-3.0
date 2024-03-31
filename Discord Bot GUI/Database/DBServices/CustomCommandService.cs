﻿using AutoMapper;
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

namespace Discord_Bot.Database.DBServices
{
    public class CustomCommandService(IMapper mapper, Logging logger, Cache cache, ICustomCommandRepository customCommandRepository, IServerRepository serverRepository) : BaseService(mapper, logger, cache), ICustomCommandService
    {
        private readonly ICustomCommandRepository customCommandRepository = customCommandRepository;
        private readonly IServerRepository serverRepository = serverRepository;

        public async Task<DbProcessResultEnum> AddCustomCommandAsync(ulong serverId, string commandName, string link)
        {
            try
            {
                if (await customCommandRepository.ExistsAsync(
                    cc => cc.Server.DiscordId == serverId.ToString() &&
                    cc.Command.Trim().ToLower().Equals(commandName.Trim().ToLower()),
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
                logger.Error("CustomCommandService.cs AddCustomCommandAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<CustomCommandResource> GetCustomCommandAsync(ulong serverId, string commandName)
        {
            CustomCommandResource result = null;
            try
            {
                CustomCommand command = await customCommandRepository.FirstOrDefaultAsync(
                    cc => cc.Server.DiscordId == serverId.ToString() &&
                    cc.Command.Trim().ToLower().Equals(commandName.Trim().ToLower()),
                    cc => cc.Server);
                result = mapper.Map<CustomCommand, CustomCommandResource>(command);
            }
            catch (Exception ex)
            {
                logger.Error("CustomCommandService.cs GetCustomCommandAsync", ex.ToString());
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
                    orderBy: cc => cc.Command,
                    includes: cc => cc.Server);
                result = mapper.Map<List<CustomCommand>, List<CustomCommandResource>>(commands);
            }
            catch (Exception ex)
            {
                logger.Error("CustomCommandService.cs GetServerCustomCommandListAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> RemoveCustomCommandAsync(ulong serverId, string commandName)
        {
            try
            {
                CustomCommand customCommand = await customCommandRepository.FirstOrDefaultAsync(
                    cc => cc.Server.DiscordId == serverId.ToString() &&
                    cc.Command.Trim().ToLower().Equals(commandName.Trim().ToLower()),
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
                logger.Error("CustomCommandService.cs RemoveCustomCommandAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}
