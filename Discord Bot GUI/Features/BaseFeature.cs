using Discord.Commands;
using Discord_Bot.Core;
using System.Threading.Tasks;

namespace Discord_Bot.Features
{
    public abstract class BaseFeature(Logging logger)
    {
        protected readonly Logging logger = logger;
        protected SocketCommandContext Context { get; private set; }

        public async Task Run()
        {
            await ExecuteCoreLogicAsync();
        }

        public async Task Run(SocketCommandContext context)
        {
            Context = context;

            await ExecuteCoreLogicAsync();
        }

        protected abstract Task ExecuteCoreLogicAsync();
    }
}
