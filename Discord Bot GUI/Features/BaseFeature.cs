using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord.Commands;
using Discord_Bot.Tools;

namespace Discord_Bot.Features;

public abstract class BaseFeature(IServerService serverService, IBotLogger logger)
{
    protected readonly IServerService serverService = serverService;
    protected readonly IBotLogger logger = logger;

    //Set by Run before ExecuteCoreLogicAsync; features started without a context or parameters never read them
    protected ICommandContext Context { get; private set; } = null!;
    protected object Parameters { get; private set; } = null!;

    public async Task<bool> Run()
    {
        return await ExecuteCoreLogicAsync();
    }

    public async Task<bool> Run(ICommandContext context)
    {
        Context = context;

        return await ExecuteCoreLogicAsync();
    }

    public async Task<bool> Run(object parameters)
    {
        Parameters = parameters;

        return await ExecuteCoreLogicAsync();
    }

    public async Task<bool> Run(ICommandContext context, object parameters)
    {
        Context = context;
        Parameters = parameters;

        return await ExecuteCoreLogicAsync();
    }

    protected abstract Task<bool> ExecuteCoreLogicAsync();

    protected async Task<ServerResource> GetCurrentServerAsync()
    {
        //CommandHandler adds every guild it sees, so a missing server means the database could not be reached (the service logged why)
        return await serverService.GetByDiscordIdAsync(Context.Guild.Id)
            ?? throw new InvalidOperationException($"Server {Context.Guild.Id} could not be loaded from the database");
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
}
