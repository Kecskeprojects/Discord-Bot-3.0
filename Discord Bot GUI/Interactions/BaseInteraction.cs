using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;

namespace Discord_Bot.Interactions
{
    public class BaseInteraction(Logging logger, Config config) : InteractionModuleBase<SocketInteractionContext>
    {
        protected readonly Logging logger = logger;
        protected readonly Config config = config;

        protected string GetCurrentUserAvatar(ImageFormat format = ImageFormat.Png, ushort size = 512)
        {
            return Context.User.GetDisplayAvatarUrl(format, size);
        }

        protected string GetCurrentUserNickname()
        {
            return Context.Channel.GetChannelType() != ChannelType.DM
                ? (Context.User as SocketGuildUser).Nickname ?? Context.User.Username
                : Context.User.Username;
        }
    }
}
