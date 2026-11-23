using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Processors.EmbedProcessors;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Processors.EmbedProcessors;

//The list embeds of !birthday list, !greeting list, !custom list and !remind list. Discord allows 25 fields per embed,
//and a field value can't be empty; Discord.Net checks both when the embed is built
public sealed class ListEmbedProcessorTests
{
    private static readonly DateTime now = TestTimes.Now.UtcDateTime;

    [Fact]
    public void BirthdayList_GivesEachDayWithTheUsersName()
    {
        List<BirthdayResource> birthdays = [new() { Date = new DateOnly(2000, 3, 4) }, new() { Date = new DateOnly(1995, 12, 1) }];

        Embed embed = Assert.Single(BirthdayListEmbedProcessor.CreateEmbed(birthdays, ["Alice", "Bob"]));

        Assert.Equal("Server birthdays:", embed.Title);
        Assert.Equal("- **2000.03.04**: Alice\n- **1995.12.01**: Bob\n", Assert.Single(embed.Fields).Value);
    }

    //A field holds 15 birthdays (the first one 16); each of these counts fills the fields exactly
    [Theory]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(46)]
    public void BirthdayList_CountThatFillsTheFieldsExactly_ShowsThemAll(int count)
    {
        List<BirthdayResource> birthdays = [.. Enumerable.Range(0, count).Select(_ => new BirthdayResource { Date = new DateOnly(2000, 3, 4) })];

        Embed embed = Assert.Single(BirthdayListEmbedProcessor.CreateEmbed(birthdays, [.. Enumerable.Range(1, count).Select(i => $"User {i}")]));

        Assert.Equal(count, embed.Fields.Sum(field => field.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length));
        Assert.Null(embed.Footer);
    }

    [Fact]
    public void GreetingList_GivesEachGreetingsIdAndLink()
    {
        Embed embed = Assert.Single(GreetingListEmbedProcessor.CreateEmbed([new() { GreetingId = 1, Url = "https://example.com/a.gif" }, new() { GreetingId = 2, Url = "https://example.com/b.gif" }]));

        Assert.Equal(["ID:1", "ID:2"], embed.Fields.Select(field => field.Name));
        Assert.Equal("https://example.com/b.gif", embed.Fields[1].Value);
    }

    //It threw until 2026-11-23
    [Fact]
    public void GreetingList_MoreThanTwentyFive_ShowsTwentyFiveAndCountsTheRest()
    {
        List<GreetingResource> greetings = [.. Enumerable.Range(1, 30).Select(i => new GreetingResource { GreetingId = i, Url = $"https://example.com/{i}.gif" })];

        Embed embed = Assert.Single(GreetingListEmbedProcessor.CreateEmbed(greetings));

        Assert.Equal(25, embed.Fields.Length);
        Assert.Equal("5 more not shown", embed.Footer?.Text);
    }

    [Fact]
    public void CustomCommandList_GivesTheCommandsWithTheirPrefix()
    {
        Embed embed = Assert.Single(CustomCommandListEmbedProcessor.CreateEmbed([new() { Command = "hello" }, new() { Command = "wave" }]));

        Assert.Equal("Custom commands:", embed.Title);
        Assert.Equal("!hello , !wave", embed.Description);
    }

    [Fact]
    public void ReminderList_NumbersThemAndShowsEachTimeAsATimestamp()
    {
        List<ReminderResource> reminders = [new() { Date = now.AddHours(1), Message = "first" }, new() { Date = now.AddHours(2), Message = "second" }];

        Embed embed = Assert.Single(UserReminderListEmbedProcessor.CreateEmbed(reminders));

        Assert.Equal($"#1 {DiscordTools.UtcTimestamp(now.AddHours(1), TimestampTagStyles.ShortDateTime)}", embed.Fields[0].Name);
        Assert.Equal(["first", "second"], embed.Fields.Select(field => field.Value));
    }

    //It threw until 2026-11-23
    [Fact]
    public void ReminderList_MoreThanTwentyFive_ShowsTheSoonestTwentyFiveAndCountsTheRest()
    {
        List<ReminderResource> reminders = [.. Enumerable.Range(1, 30).Select(i => new ReminderResource { Date = now.AddHours(i), Message = $"reminder {i}" })];

        Embed embed = Assert.Single(UserReminderListEmbedProcessor.CreateEmbed(reminders));

        Assert.Equal(25, embed.Fields.Length);
        Assert.Equal("reminder 25", embed.Fields[^1].Value);
        Assert.Equal("5 more not shown", embed.Footer?.Text);
    }

    //It came out empty, and threw, at 16, 31, 46 and from 61 birthdays until 2026-11-23
    [Fact]
    public void BirthdayList_SeventyBirthdays_ShowsTheFirstSixtyOneAndCountsTheRest()
    {
        List<BirthdayResource> birthdays = [.. Enumerable.Range(0, 70).Select(_ => new BirthdayResource { Date = new DateOnly(2000, 3, 4) })];

        Embed embed = Assert.Single(BirthdayListEmbedProcessor.CreateEmbed(birthdays, [.. Enumerable.Range(1, 70).Select(i => $"User {i}")]));

        Assert.Equal(61, embed.Fields.Sum(field => field.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length));
        Assert.Equal("9 more not shown", embed.Footer?.Text);
    }
}
