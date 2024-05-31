using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;

namespace Discord_Bot.Database.DBServices
{
    //Todo: Consider what can be moved into this class as universal functions, cache related logics may be among the possibilities
    public class BaseService(IMapper mapper, Logging logger, Cache cache)
    {
        protected readonly IMapper mapper = mapper;
        protected readonly Logging logger = logger;
        protected readonly Cache cache = cache;
    }
}
