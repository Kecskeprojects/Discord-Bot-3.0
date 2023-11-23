using Discord.Commands;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;

namespace Discord_Bot.Commands
{
    public class CommandBase(Logging logger, Config config) : ModuleBase<SocketCommandContext>
    {
        protected readonly Logging logger = logger;
        protected readonly Config config = config;
    }
}
