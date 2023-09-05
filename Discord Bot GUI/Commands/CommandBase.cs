using Discord.Commands;
using Discord_Bot.Core.Logger;

namespace Discord_Bot.Commands
{
    public class CommandBase : ModuleBase<SocketCommandContext>
    {
        protected readonly Logging logger;

        public CommandBase(Logging logger) 
        {
            this.logger = logger;
        }
    }
}
