using Discord.Commands;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;

namespace Discord_Bot.Commands
{
    public class CommandBase : ModuleBase<SocketCommandContext>
    {
        protected readonly Logging logger;
        protected readonly Config config;

        public CommandBase(Logging logger, Config config)
        {
            this.logger = logger;
            this.config = config;
        }
    }
}
