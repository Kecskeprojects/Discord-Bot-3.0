using Discord;
using Discord.Net;
using System.Net;

namespace Discord_Bot.Tests.Mocks.Discord;

//The exceptions Discord.Net throws when Discord refuses a request, for making a substitute fail the way Discord does
public static class DiscordErrors
{
    //50007: the user's direct messages are closed, or they share no server with the bot
    public static HttpException ClosedDirectMessages()
    {
        return new HttpException(HttpStatusCode.Forbidden, null, DiscordErrorCode.CannotSendMessageToUser);
    }

    //50013: the bot may not post in the channel or change the role
    public static HttpException MissingPermissions()
    {
        return new HttpException(HttpStatusCode.Forbidden, null, DiscordErrorCode.InsufficientPermissions);
    }
}
