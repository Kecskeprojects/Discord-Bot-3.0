using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using MapsterMapper;

namespace Discord_Bot.Database.DBServices;

public class BaseService(IMapper mapper, BotLogger logger, ServerCache cache)
{
    protected readonly IMapper mapper = mapper;
    protected readonly BotLogger logger = logger;
    protected readonly ServerCache cache = cache;
}
