using Discord;
using Discord.Interactions;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System.Threading.Tasks;

namespace Discord_Bot.Interactions;
public class BaseInteraction(IServerService serverService, BotLogger logger, Config config) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServerService serverService = serverService;
    protected readonly BotLogger logger = logger;
    protected readonly Config config = config;

    protected Task<bool> IsOwner()
    {
        return DiscordTools.IsOwner(Context);
    }

    protected bool IsAdmin()
    {
        return DiscordTools.IsAdmin(Context);
    }

    protected string GetCurrentUserAvatar(ImageFormat format = ImageFormat.Png, ushort size = 512)
    {
        return Context.User.GetDisplayAvatarUrl(format, size);
    }

    protected string GetCurrentUserNickname()
    {
        return DiscordTools.GetNickName(Context);
    }

    protected async Task<bool> IsCommandAllowedAsync(ChannelTypeEnum type, bool allowLackOfType = true, bool canBeDM = false)
    {
        if (DiscordTools.IsDM(Context))
        {
            return canBeDM;
        }

        ServerResource server = await GetCurrentServerAsync();
        return DiscordTools.IsTypeOfChannel(server, type, Context.Channel.Id, allowLackOfType);
    }

    protected async Task<ServerResource> GetCurrentServerAsync()
    {
        return await serverService.GetByDiscordIdAsync(Context.Guild.Id);
    }
}
