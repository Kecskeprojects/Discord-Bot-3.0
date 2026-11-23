using System.Globalization;
using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Core;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Mocks.Discord;

namespace Discord_Bot.Tests.Unit.Bot.Processors.MessageProcessor;

//The daily birthday greeting, picked at random from Constant.BirthdayMessage
public sealed class BirthdayMessageProcessorTests
{
    private static readonly IUser user = DiscordMocks.User();

    [Fact]
    public void EveryGreeting_FormatsWithTheMentionAndTheAge()
    {
        foreach (string greeting in Constant.BirthdayMessage)
        {
            Assert.Contains(user.Mention, string.Format(CultureInfo.InvariantCulture, greeting, user.Mention, "26"), StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CreateMessage_GivesOneOfTheGreetingsWithTheAgeThisYear()
    {
        HashSet<string> greetings = [.. Constant.BirthdayMessage.Select(greeting => string.Format(CultureInfo.InvariantCulture, greeting, user.Mention, "26"))];
        BirthdayResource birthday = new() { UserDiscordId = DiscordIds.User, Date = new DateOnly(2000, 3, 4) };

        //The pick isn't injected yet, so a few runs cover several greetings
        for (int i = 0; i < 20; i++)
        {
            Assert.Contains(BirthdayMessageProcessor.CreateMessage(birthday, user, 2026), greetings);
        }
    }
}
