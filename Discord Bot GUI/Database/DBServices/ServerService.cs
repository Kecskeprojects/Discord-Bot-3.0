using AutoMapper;
using Discord_Bot.Core.Logger;
using Discord_Bot.Database.Models;
using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBServices
{
    public class ServerService : BaseService, IServerService
    {
        private readonly IServerRepository serverRepository;

        public ServerService(IServerRepository serverRepository, IMapper mapper, Logging logger) : base(mapper, logger)
        {
            this.serverRepository = serverRepository;
        }

        public async Task<List<ServerResource>> GetAllServerAsync()
        {
            List<Server> result = await serverRepository.GetAllServerAsync();
            return mapper.Map<List<Server>, List<ServerResource>>(result);
        }
    }
}
