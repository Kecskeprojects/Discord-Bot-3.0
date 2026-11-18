using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Interactions;
using Discord_Bot.Interfaces;
using Discord_Bot.Tools;

namespace Discord_Bot.Interactions;

public class BaseInteraction(IServerService serverService, IBotLogger logger, IConfig config) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServerService serverService = serverService;
    protected readonly IBotLogger logger = logger;
    protected readonly IConfig config = config;

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
        //CommandHandler adds every guild it sees, so a missing server means the database could not be reached (the service logged why)
        return await serverService.GetByDiscordIdAsync(Context.Guild.Id)
            ?? throw new InvalidOperationException($"Server {Context.Guild.Id} could not be loaded from the database");
    }

    //Usable whether or not the interaction was already acknowledged (e.g. in a catch block after DeferAsync)
    protected Task RespondOrFollowupAsync(string text, bool ephemeral = false)
    {
        return DiscordTools.RespondOrFollowupAsync(Context.Interaction, text, ephemeral);
    }
}
