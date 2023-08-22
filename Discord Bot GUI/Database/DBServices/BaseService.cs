using AutoMapper;
using Discord_Bot.Core.Caching;
using Discord_Bot.Core.Logger;

namespace Discord_Bot.Database.DBServices
{
    public class BaseService
    {
        protected readonly IMapper mapper;
        protected readonly Logging logger;
        protected readonly Cache cache;
        public BaseService(IMapper mapper, Logging logger, Cache cache)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.cache = cache;
        }
    }
}
