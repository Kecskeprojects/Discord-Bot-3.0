using AutoMapper;
using Discord_Bot.Core;
using Discord_Bot.Core.Caching;
using Discord_Bot.Interfaces.DBServices;

namespace Discord_Bot.Database.DBServices
{
    internal class IdolGroupService(IMapper mapper, Logging logger, Cache cache) : BaseService(mapper, logger, cache), IIdolGroupService
    {
    }
}
