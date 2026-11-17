using DatabaseORM.Enums;
using DatabaseORM.Interface;
using DatabaseORM.Interface.Service;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;
using Discord_Bot.Communication;
using Discord_Bot.Core;
using Discord_Bot.Core.Configuration;
using Discord_Bot.Tools;

namespace Discord_Bot.Commands;

public class BaseCommand(IBotLogger logger, Config config, IServerService serverService) : ModuleBase<SocketCommandContext>
{
    protected readonly IBotLogger logger = logger;
    protected readonly Config config = config;
    protected readonly IServerService serverService = serverService;

    protected async Task<ServerResource> GetCurrentServerAsync()
    {
        //CommandHandler adds every guild it sees, so a missing server means the database could not be reached (the service logged why)
        return await serverService.GetByDiscordIdAsync(Context.Guild.Id)
            ?? throw new InvalidOperationException($"Server {Context.Guild.Id} could not be loaded from the database");
    }

    protected string GetCurrentUserAvatar(ImageFormat format = ImageFormat.Png, ushort size = 512)
    {
        return DiscordTools.GetUserAvatarUrl(Context.User, format, size);
    }

    protected Task<bool> IsOwner()
    {
        return DiscordTools.IsOwner(Context);
    }

    protected bool IsAdmin()
    {
        return DiscordTools.IsAdmin(Context);
    }

    protected string GetCurrentUserNickname()
    {
        return DiscordTools.GetNickName(Context);
    }

    protected string GetUserNickname(IUser user)
    {
        return DiscordTools.GetNickName(Context, user);
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

    protected ServerAudioResource GetCurrentAudioResource()
    {
        if (!Global.ServerAudioResources.TryGetValue(Context.Guild.Id, out ServerAudioResource? audioResource))
        {
            audioResource = new(Context.Guild.Id);
            _ = Global.ServerAudioResources.TryAdd(Context.Guild.Id, audioResource);
        }
        return audioResource;
    }

    protected static string[] GetParametersBySplit(string parameters, char splitCharacter = '>', bool toLower = true)
    {
        if (toLower)
        {
            parameters = parameters.ToLower();
        }

        return parameters.Split(splitCharacter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    protected static string[] GetParametersBySplit(string parameters, string splitCharacter = ">", bool toLower = true)
    {
        if (toLower)
        {
            parameters = parameters.ToLower();
        }

        return parameters.Split(splitCharacter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    protected static string[] GetDateParameterParts(string parameters)
    {
        return parameters.Split(Constant.DateSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
