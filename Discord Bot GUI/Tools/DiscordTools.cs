﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
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

    public static Task<bool> IsOwner(SocketInteractionContext context)
    {
        return IsOwnerBase(context.Client, context.User.Id);
    }

    public static Task<bool> IsOwner(SocketCommandContext context)
    {
        return IsOwnerBase(context.Client, context.User.Id);
    }

    private static async Task<bool> IsOwnerBase(DiscordSocketClient client, ulong userId)
    {
        RestApplication application = await client.GetApplicationInfoAsync();
        return userId == application.Owner.Id;
    }

    public static bool IsAdmin(SocketInteractionContext context)
    {
        return IsAdminBase(context.User);
    }

    public static bool IsAdmin(SocketCommandContext context)
    {
        return IsAdminBase(context.User);
    }

    private static bool IsAdminBase(SocketUser user)
    {
        return user is IGuildUser guildUser && guildUser.GuildPermissions.Has(GuildPermission.Administrator);
    }

    public static bool IsDM(SocketInteractionContext context)
    {
        return IsDMBase(context.Channel);
    }

    public static bool IsDM(SocketCommandContext context)
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

    public static string GetUserAvatarUrl(IUser user, ImageFormat format = ImageFormat.Png, ushort size = 512)
    {
        return user.GetDisplayAvatarUrl(format, size);
    }
}
