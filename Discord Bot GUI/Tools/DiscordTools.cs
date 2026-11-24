using DatabaseORM.Enums;
using DatabaseORM.Resources;
using Discord;
using Discord.Commands;

namespace Discord_Bot.Tools;

public static class DiscordTools
{
    //For a server that set no channel of this type, allowLackOfType decides: commands are allowed in every channel,
    //while CommandHandler's role channel needs to be set (it deletes every other message there)
    public static bool IsTypeOfChannel(ServerResource server, ChannelTypeEnum type, ulong channelId, bool allowLackOfType = true)
    {
        return !server.SettingsChannels.TryGetValue(type, out List<ulong>? value)
            ? allowLackOfType
            : value.Contains(channelId);
    }

    public static Task<bool> IsOwner(IInteractionContext context)
    {
        return IsOwnerBase(context.Client, context.User.Id);
    }

    public static Task<bool> IsOwner(ICommandContext context)
    {
        return IsOwnerBase(context.Client, context.User.Id);
    }

    private static async Task<bool> IsOwnerBase(IDiscordClient client, ulong userId)
    {
        IApplication application = await client.GetApplicationInfoAsync();
        return userId == application.Owner.Id;
    }

    public static bool IsAdmin(IInteractionContext context)
    {
        return IsAdminBase(context.User);
    }

    public static bool IsAdmin(ICommandContext context)
    {
        return IsAdminBase(context.User);
    }

    private static bool IsAdminBase(IUser user)
    {
        return user is IGuildUser guildUser && guildUser.GuildPermissions.Has(GuildPermission.Administrator);
    }

    public static bool IsDM(IInteractionContext context)
    {
        return IsDMBase(context.Channel);
    }

    public static bool IsDM(ICommandContext context)
    {
        return IsDMBase(context.Channel);
    }

    public static bool IsDM(IMessage message)
    {
        return IsDMBase(message.Channel);
    }

    public static bool IsDMBase(IMessageChannel channel)
    {
        return channel.GetChannelType() == ChannelType.DM;
    }

    public static string GetNickName(IInteractionContext context, IUser? user = null)
    {
        user ??= context.User;
        return GetNickNameBase(user, context.Channel);
    }

    public static string GetNickName(ICommandContext context, IUser? user = null)
    {
        user ??= context.User;
        return GetNickNameBase(user, context.Channel);
    }

    public static string GetNickNameBase(IUser user, IMessageChannel channel)
    {
        return !IsDMBase(channel) ?
            (user as IGuildUser)?.Nickname ?? user.Username :
            user.Username;
    }

    //Only the users in the server's member cache, as before; nothing is downloaded per user
    public static async Task<List<UserResource>> FilterToOnlyServerMembersAsync(ICommandContext context, List<UserResource> users)
    {
        List<UserResource> filtered = [];
        foreach (UserResource item in users)
        {
            //Check if user is in given server
            IGuildUser? guildUser = await context.Guild.GetUserAsync(item.DiscordId, CacheMode.CacheOnly);
            if (guildUser != null)
            {
                //Get their nickname if they have one
                item.Username = GetNickNameBase(guildUser, context.Channel);
                filtered.Add(item);
            }
        }

        return filtered;
    }

    public static string GetUserAvatarUrl(IUser user, ImageFormat format = ImageFormat.Png, ushort size = 512)
    {
        return user.GetDisplayAvatarUrl(format, size);
    }

    //Discord shows a timestamp in each reader's own time zone and language
    //The bot's times are UTC, but the database returns them without a kind, which TimestampTag would read as the machine's local time
    public static TimestampTag UtcTimestamp(DateTime utcTime, TimestampTagStyles style)
    {
        return TimestampTag.FromDateTime(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), style);
    }

    //An interaction can only be responded to once, after that (including after DeferAsync) only followups are accepted
    public static async Task RespondOrFollowupAsync(IDiscordInteraction interaction, string text, bool ephemeral = false)
    {
        if (interaction.HasResponded)
        {
            _ = await interaction.FollowupAsync(text, ephemeral: ephemeral);
        }
        else
        {
            await interaction.RespondAsync(text, ephemeral: ephemeral);
        }
    }
}
