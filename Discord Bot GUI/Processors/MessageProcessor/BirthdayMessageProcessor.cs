using Discord.WebSocket;
using Discord_Bot.Enums;
using Discord_Bot.Resources;
using System;

namespace Discord_Bot.Processors.MessageProcessor;

public static class BirthdayMessageProcessor
{
    public static string CreateMessage(BirthdayResource birthday, SocketGuild guild)
    {
        SocketGuildUser user = guild.GetUser(birthday.UserDiscordId);

        Random r = new();
        string baseMessage = StaticLists.BirthdayMessage[r.Next(0, StaticLists.BirthdayMessage.Length)];

        string message = string.Format(baseMessage, user.Mention, (DateTime.UtcNow.Year - birthday.Date.Year).ToString());
        return message;
    }
}
