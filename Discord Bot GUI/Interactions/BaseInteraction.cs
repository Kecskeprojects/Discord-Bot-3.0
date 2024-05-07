using Discord.Interactions;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;

namespace Discord_Bot.Interactions
{
    public class BaseInteraction(Logging logger, Config config) : InteractionModuleBase<SocketInteractionContext>
    {
        protected readonly Logging logger = logger;
        protected readonly Config config = config;
    }
}
