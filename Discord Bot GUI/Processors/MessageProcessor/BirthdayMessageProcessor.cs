using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Core;
using System.Globalization;

namespace Discord_Bot.Processors.MessageProcessor;

public static class BirthdayMessageProcessor
{
    public static string CreateMessage(BirthdayResource birthday, IUser user, int currentYear)
    {
        Random r = new();
        string baseMessage = Constant.BirthdayMessage[r.Next(0, Constant.BirthdayMessage.Length)];

        string message = string.Format(CultureInfo.InvariantCulture, baseMessage, user.Mention, (currentYear - birthday.Date.Year).ToString());
        return message;
    }
}
