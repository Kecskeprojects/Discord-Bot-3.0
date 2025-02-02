using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;

namespace Discord_Bot.Database.DBServices;

public class BaseService(IMapper mapper, BotLogger logger, ServerCache cache)
{
    protected readonly IMapper mapper = mapper;
    protected readonly BotLogger logger = logger;
    protected readonly ServerCache cache = cache;

    //Todo: Turn BaseService into a Generic class that can be used to make simple db calls even simpler, they will still need to be defined in their respective classes
}
