using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord_Bot.Communication;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Tools;

public static class DiscordTools
{
    public static bool IsTypeOfChannel(ServerResource server, ChannelTypeEnum type, ulong channelId, bool allowLackOfType = true)
    {
        return server == null || (!server.SettingsChannels.TryGetValue(type, out List<ulong> value)
            ? allowLackOfType
            : value.Contains(channelId));
    }

    public static bool IsDM(SocketCommandContext context)
    {
        return IsDMBase(context.Channel);
    }

    public static bool IsDM(SocketInteractionContext context)
    {
        return IsDMBase(context.Channel);
    }

    public static bool IsDM(SocketMessage message)
    {
        return IsDMBase(message.Channel);
    }

    public static bool IsDMBase(ISocketMessageChannel channel)
    {
        return channel.GetChannelType() == ChannelType.DM;
    }

    public static string GetNickName(SocketInteractionContext context, IUser user = null)
    {
        user ??= context.User;
        return GetNickNameBase(user, context.Channel);
    }

    public static string GetNickName(SocketCommandContext context, IUser user = null)
    {
        user ??= context.User;
        return GetNickNameBase(user, context.Channel);
    }

    public static string GetNickNameBase(IUser user, ISocketMessageChannel channel)
    {
        return !IsDMBase(channel) ?
            (user as SocketGuildUser).Nickname ?? user.Username :
            user.Username;
    }

    public static async Task<bool> ConnectBot(SocketCommandContext context, ServerAudioResource audioResource, ServerResource server)
    {
        SocketGuildUser clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id) as SocketGuildUser;

        SocketVoiceChannel channel = (context.User as SocketGuildUser).VoiceChannel;

        if (clientUser.VoiceChannel != null)
        {
            await clientUser.VoiceChannel.DisconnectAsync();
        }

        if (channel != null && IsTypeOfChannel(server, ChannelTypeEnum.MusicVoice, channel.Id, allowLackOfType: true))
        {
            audioResource.AudioVariables.AudioClient = await channel.ConnectAsync();

            if (audioResource.AudioVariables.AudioClient != null)
            {
                audioResource.AudioVariables.FallbackVoiceChannelId = channel.Id;
                return true;
            }
        }
        return false;
    }

    public static async Task<bool> ReConnectBot(SocketCommandContext context, ServerAudioResource audioResource)
    {
        SocketVoiceChannel channel = context.Guild.GetVoiceChannel(audioResource.AudioVariables.FallbackVoiceChannelId);

        audioResource.AudioVariables.AudioClient = await channel.ConnectAsync();

        if (audioResource.AudioVariables.AudioClient != null)
        {
            audioResource.AudioVariables.AbruptDisconnect = false;
            return true;
        }
        return false;
    }

    public static List<UserResource> FilterToOnlyServerMembers(SocketCommandContext context, List<UserResource> users)
    {
        List<UserResource> filtered = [];
        foreach (UserResource item in users)
        {
            //Check if user is in given server
            SocketGuildUser temp_user = context.Guild.GetUser(item.DiscordId);
            if (temp_user != null)
            {
                //Get their nickname if they have one
                item.Username = GetNickNameBase(temp_user, context.Channel);
                filtered.Add(item);
            }
        }

        return filtered;
    }
}
