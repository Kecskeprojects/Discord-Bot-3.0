using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System.Threading.Tasks;

namespace Discord_Bot.Commands;

//Todo: after reorganizing, check if anything in especially the longer commands can be moved into tools, processors, etc...
public class BaseCommand(BotLogger logger, Config config, IServerService serverService) : ModuleBase<SocketCommandContext>
{
    protected readonly BotLogger logger = logger;
    protected readonly Config config = config;
    protected readonly IServerService serverService = serverService;

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
        return DiscordTools.GetNickName(Context);
    }

    protected string GetUserNickname(IUser user)
    {
        return DiscordTools.GetNickName(Context, user);
    }

    protected async Task<bool> IsCommandAllowedAsync(ChannelTypeEnum type, bool allowLackOfType = true)
    {
        if (DiscordTools.IsDM(Context))
        {
            return false;
        }

        ServerResource server = await GetCurrentServerAsync();
        return DiscordTools.IsTypeOfChannel(server, type, Context.Channel.Id, allowLackOfType);
    }

    protected ServerAudioResource GetCurrentAudioResource()
    {
        if (!Global.ServerAudioResources.TryGetValue(Context.Guild.Id, out ServerAudioResource audioResource))
        {
            audioResource = new(Context.Guild.Id);
            Global.ServerAudioResources.TryAdd(Context.Guild.Id, audioResource);
        }
        return audioResource;
    }
}
