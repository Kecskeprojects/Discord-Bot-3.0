using DatabaseORM.Caching;
using DatabaseORM.Interface;
using MapsterMapper;

namespace DatabaseORM.Service;

internal class BaseService(IMapper mapper, IBotLogger logger, ServerCache cache)
{
    protected readonly IMapper mapper = mapper;
    protected readonly IBotLogger logger = logger;
    protected readonly ServerCache cache = cache;
}
