using Discord.Interactions;
using Discord_Bot.Core.Config;
using Discord_Bot.Core.Logger;

namespace Discord_Bot.Interactions
{
    public class BaseInteraction(Logging logger, Config config) : InteractionModuleBase<SocketInteractionContext>
    {
        protected readonly Logging logger = logger;
        protected readonly Config config = config;
    }
}
