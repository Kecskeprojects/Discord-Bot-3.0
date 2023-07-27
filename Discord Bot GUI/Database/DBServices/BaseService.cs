using AutoMapper;
using Discord_Bot.Logger;

namespace Discord_Bot.Database.DBServices
{
    public class BaseService
    {
        protected readonly IMapper mapper;
        protected readonly Logging logger;
        public BaseService(IMapper mapper, Logging logger)
        {
            this.mapper = mapper;
            this.logger = logger;
        }
    }
}
