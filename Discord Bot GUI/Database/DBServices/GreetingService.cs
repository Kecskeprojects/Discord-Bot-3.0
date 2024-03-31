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

namespace Discord_Bot.Database.DBServices
{
    public class GreetingService(IGreetingRepository greetingRepository, IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IGreetingService
    {
        private readonly IGreetingRepository greetingRepository = greetingRepository;

        public async Task<List<GreetingResource>> GetAllGreetingAsync()
        {
            List<GreetingResource> result = null;
            try
            {
                List<Greeting> greetings = await greetingRepository.GetAllAsync();
                result = mapper.Map<List<Greeting>, List<GreetingResource>>(greetings);
            }
            catch (Exception ex)
            {
                logger.Error("GreetingService.cs GetAllGreetingAsync", ex.ToString());
            }
            return result;
        }

        public async Task<DbProcessResultEnum> AddGreetingAsync(string url)
        {
            try
            {
                Greeting greeting = new()
                {
                    GreetingId = 0,
                    Url = url
                };
                await greetingRepository.AddAsync(greeting);

                logger.Log("Greeting added successfully!");
                return DbProcessResultEnum.Success;
            }
            catch (Exception ex)
            {
                logger.Error("GreetingService.cs AddGreetingAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }

        public async Task<DbProcessResultEnum> RemoveGreetingAsync(int id)
        {
            try
            {
                Greeting greeting = await greetingRepository.FindByIdAsync(id);
                if (greeting != null)
                {
                    await greetingRepository.RemoveAsync(greeting);

                    logger.Log($"Greeting with ID: {id} removed successfully!");
                    return DbProcessResultEnum.Success;
                }
                else
                {
                    logger.Log($"Greeting with ID: {id} not found!");
                    return DbProcessResultEnum.NotFound;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GreetingService.cs RemoveGreetingAsync", ex.ToString());
            }
            return DbProcessResultEnum.Failure;
        }
    }
}
