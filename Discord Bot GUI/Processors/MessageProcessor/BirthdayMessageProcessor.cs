using Discord.WebSocket;
using Discord_Bot.Core;
using Discord_Bot.Resources;
using System;

namespace Discord_Bot.Processors.MessageProcessor;
public static class BirthdayMessageProcessor
{
    public static string CreateMessage(BirthdayResource birthday, SocketGuild guild)
    {
        SocketGuildUser user = guild.GetUser(birthday.UserDiscordId);

        Random r = new();
        string baseMessage = Constant.BirthdayMessage[r.Next(0, Constant.BirthdayMessage.Length)];

        string message = string.Format(baseMessage, user.Mention, (DateTime.UtcNow.Year - birthday.Date.Year).ToString());
        return message;
    }
}
