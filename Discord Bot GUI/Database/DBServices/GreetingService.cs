using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class GreetingService : BaseService, IGreetingService
    {
        private readonly IGreetingRepository greetingRepository;
        public GreetingService(IGreetingRepository greetingRepository, IMapper mapper, Logging logger, Cache cache) : base(mapper, logger, cache) => this.greetingRepository = greetingRepository;

        public async Task<List<GreetingResource>> GetAllGreetingAsync()
        {
            List<GreetingResource> result = null;
            try
            {
                List<Greeting> greetings = await greetingRepository.GetAllGreetingAsync();
                result = mapper.Map<List<Greeting>, List<GreetingResource>>(greetings);
            }
            catch (Exception ex)
            {
                logger.Error("GreetingService.cs GetAllGreetingAsync", ex.ToString());
            }
            return result;
        }
    }
}
