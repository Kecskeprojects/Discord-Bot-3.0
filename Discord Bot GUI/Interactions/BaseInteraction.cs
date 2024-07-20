using Discord;
using Discord.Interactions;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Tools;

namespace Discord_Bot.Interactions;
//Todo: after reorganizing, check if anything in especially the longer interactions can be moved into tools, processors, etc...
public class BaseInteraction(BotLogger logger, Config config) : InteractionModuleBase<SocketInteractionContext>
{
    protected readonly BotLogger logger = logger;
    protected readonly Config config = config;

    protected string GetCurrentUserAvatar(ImageFormat format = ImageFormat.Png, ushort size = 512)
    {
        return Context.User.GetDisplayAvatarUrl(format, size);
    }

    protected string GetCurrentUserNickname()
    {
        return DiscordTools.GetNickName(Context);
    }
}
