﻿using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Enums;
using Discord_Bot.Interfaces.DBServices;
using Discord_Bot.Resources;
using Discord_Bot.Tools;
using System.Threading.Tasks;

namespace Discord_Bot.Features;
public abstract class BaseFeature(IServerService serverService, BotLogger logger)
{
    protected readonly IServerService serverService = serverService;
    protected readonly BotLogger logger = logger;

    protected SocketCommandContext Context { get; private set; }
    protected object Parameters { get; private set; }

    public async Task<bool> Run()
    {
        return await ExecuteCoreLogicAsync();
    }

    public async Task<bool> Run(SocketCommandContext context)
    {
        Context = context;

        return await ExecuteCoreLogicAsync();
    }

    public async Task<bool> Run(object parameters)
    {
        Parameters = parameters;

        return await ExecuteCoreLogicAsync();
    }

    public async Task<bool> Run(SocketCommandContext context, object parameters)
    {
        Context = context;
        Parameters = parameters;

        return await ExecuteCoreLogicAsync();
    }

    protected abstract Task<bool> ExecuteCoreLogicAsync();

    protected ServerAudioResource GetCurrentAudioResource()
    {
        if (!Global.ServerAudioResources.TryGetValue(Context.Guild.Id, out ServerAudioResource audioResource))
        {
            audioResource = new(Context.Guild.Id);
            Global.ServerAudioResources.TryAdd(Context.Guild.Id, audioResource);
        }
        return audioResource;
    }

    protected async Task<ServerResource> GetCurrentServerAsync()
    {
        return await serverService.GetByDiscordIdAsync(Context.Guild.Id);
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
