using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System.Threading.Tasks;

namespace Discord_Bot.Commands;

//Todo: after reorganizing, check if anything in especially the longer commands can be moved into tools, processors, etc...
public class BaseCommand(Logging logger, Config config, IServerService serverService) : ModuleBase<SocketCommandContext>
{
    protected readonly Logging logger = logger;
    protected readonly Config config = config;
    protected readonly IServerService serverService = serverService;

    protected bool IsDM()
    {
        return Context.Channel.GetChannelType() == ChannelType.DM;
    }

    protected async Task<ServerResource> GetCurrentServerAsync()
    {
        return await serverService.GetByDiscordIdAsync(Context.Guild.Id);
    }

    protected string GetCurrentUserAvatar(ImageFormat format = ImageFormat.Png, ushort size = 512)
    {
        return Context.User.GetDisplayAvatarUrl(format, size);
    }

    protected static string GetUserAvatar(IUser user, ImageFormat format = ImageFormat.Png, ushort size = 512)
    {
        return user.GetDisplayAvatarUrl(format, size);
    }

    protected string GetCurrentUserNickname()
    {
        return !IsDM()
            ? (Context.User as SocketGuildUser).Nickname ?? Context.User.Username
            : Context.User.Username;
    }

    protected string GetUserNickname(IUser user)
    {
        return !IsDM()
            ? (user as SocketGuildUser).Nickname ?? user.Username
            : user.Username;
    }

    protected async Task<bool> IsCommandAllowedAsync(ChannelTypeEnum type, bool allowLackOfType = true)
    {
        if (IsDM())
        {
            return false;
        }

        ServerResource server = await GetCurrentServerAsync();
        return DiscordTools.IsTypeOfChannel(server, type, Context.Channel.Id, allowLackOfType);
    }
}
