using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class CustomCommandService : BaseService, ICustomCommandService
    {
        private readonly ICustomCommandRepository customCommandRepository;

        public CustomCommandService(IMapper mapper, Logging logger, Cache cache, ICustomCommandRepository customCommandRepository) : base(mapper, logger, cache)
        {
            this.customCommandRepository = customCommandRepository;
        }

        public async Task<CustomCommandResource> GetCustomCommandAsync(ulong id, string commandName)
        {
            CustomCommandResource result = null;
            try
            {
                CustomCommand command = await customCommandRepository.GetCustomCommandAsync(id, commandName);
                result = mapper.Map<CustomCommand, CustomCommandResource>(command);
            }
            catch(Exception ex)
            {
                logger.Error("CustomCommandService.cs GetCustomCommandAsync", ex.ToString());
            }
            return result;
        }
    }
}
