using Discord.Commands;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;
using Discord_Bot.Interfaces.DBServices;

namespace Discord_Bot.Commands
{
    public class BaseCommand(Logging logger, Config config, IServerService serverService) : ModuleBase<SocketCommandContext>
    {
        protected readonly Logging logger = logger;
        protected readonly Config config = config;
        protected readonly IServerService serverService = serverService;
    }
}
