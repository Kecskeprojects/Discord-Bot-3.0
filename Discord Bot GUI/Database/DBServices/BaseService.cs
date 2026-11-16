using Discord_Bot.Core.Caching;
using Discord_Bot.Interfaces.Core;
using MapsterMapper;

namespace Discord_Bot.Database.DBServices;

public class BaseService(IMapper mapper, IBotLogger logger, ServerCache cache)
{
    protected readonly IMapper mapper = mapper;
    protected readonly IBotLogger logger = logger;
    protected readonly ServerCache cache = cache;
}
