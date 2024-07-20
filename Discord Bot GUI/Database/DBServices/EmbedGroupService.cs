using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Interfaces.DBServices;

namespace Discord_Bot.Database.DBServices;
public class EmbedGroupService(
    IMapper mapper,
    BotLogger logger,
    ServerCache cache) : BaseService(mapper, logger, cache), IEmbedGroupService
{
}
