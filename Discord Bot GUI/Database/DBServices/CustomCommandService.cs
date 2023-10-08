using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
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
    public class CustomCommandService : BaseService, ICustomCommandService
    {
        private readonly ICustomCommandRepository customCommandRepository;
        private readonly IServerRepository serverRepository;

        public CustomCommandService(IMapper mapper, Logging logger, Cache cache, ICustomCommandRepository customCommandRepository, IServerRepository serverRepository) : base(mapper, logger, cache)
        {
            this.customCommandRepository = customCommandRepository;
            this.serverRepository = serverRepository;
        }

        public async Task<DbProcessResultEnum> AddCustomCommandAsync(ulong serverId, string commandName, string link)
        {
            try
            {
                if (await customCommandRepository.CustomCommandExistsAsync(serverId, commandName))
                {
                    return DbProcessResultEnum.AlreadyExists;
                }

                Server server = await serverRepository.GetByDiscordIdAsync(serverId);

                CustomCommand command = new()
                {
                    CommandId = 0,
                    Server = server,
                    Command = commandName,
                    Url = link

                };
                await customCommandRepository.AddCustomCommandAsync(command);

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
                CustomCommand command = await customCommandRepository.GetCustomCommandAsync(serverId, commandName);
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
                List<CustomCommand> commands = await customCommandRepository.GetCustomCommandListAsync(serverId);
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
                CustomCommand customCommand = await customCommandRepository.GetCustomCommandAsync(serverId, commandName);
                if (customCommand != null)
                {
                    await customCommandRepository.RemoveCustomCommandAsync(customCommand);

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
