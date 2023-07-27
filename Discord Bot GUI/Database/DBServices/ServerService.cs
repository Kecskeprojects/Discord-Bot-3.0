using AutoMapper;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Logger;

namespace Discord_Bot.Database.DBServices
{
    public class ServerService : BaseService, IServerService
    {
        private readonly IServerRepository serverRepository;

        public ServerService(IServerRepository serverRepository, IMapper mapper, Logging logger) : base(mapper, logger)
        {
            this.serverRepository = serverRepository;
        }
    }
}
