using DatabaseORM.Resources;
using Discord;
using Discord_Bot.Communication.Bias;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Tests.Common.Constants;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Processors.MessageProcessor;

//!my biases and !bias list: one group's members as text, or a menu of the groups, and the members of the group picked from it
public sealed class BiasListMessageProcessorTests
{
    [Fact]
    public void BuildBiasMessage_GroupGiven_ListsItsMembersInCapitals()
    {
        BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage([Idol("jisoo", "blackpink"), Idol("jennie", "blackpink")], "blackpink", "head", DiscordIds.User, true);

        Assert.Equal("BLACKPINK:\n`JISOO`, `JENNIE`\n", result.Message);
        Assert.Null(result.Component);
    }

    [Fact]
    public void BuildBiasMessage_NoGroup_OffersTheGroupsByNameWithTheUsersId()
    {
        List<IdolResource> idols = [Idol("karina", "aespa"), Idol("jisoo", "blackpink"), Idol("jennie", "blackpink"), Idol("winter", "aespa")];

        BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage(idols, "", "head", DiscordIds.User, true);

        Assert.Equal("head", result.Message);
        SelectMenuComponent menu = Assert.Single(Menus(result));
        Assert.Equal("BiasMenu_1", menu.CustomId);
        Assert.Equal(["AESPA", "BLACKPINK"], menu.Options.Select(option => option.Label));
        //BiasListComponentInteraction reads the user's ID back from the value
        Assert.Equal([$"aespa><{DiscordIds.User}", $"blackpink><{DiscordIds.User}"], menu.Options.Select(option => option.Value));
        Assert.Equal(["2 biases...", "2 biases..."], menu.Options.Select(option => option.Description));
    }

    //Hungarian sorts "cs" as a letter of its own after "c", so until 2026-11-24 the order followed the machine's culture
    [Theory]
    [InlineData("en-US")]
    [InlineData("hu-HU")]
    public void BuildBiasMessage_InAnyCulture_OrdersTheGroupsTheSame(string culture)
    {
        using CultureScope scope = new(culture);

        BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage([Idol("a", "cube"), Idol("b", "csr")], "", "head", DiscordIds.User, false);

        Assert.Equal(["CSR", "CUBE"], Assert.Single(Menus(result)).Options.Select(option => option.Label));
    }

    [Fact]
    public void BuildBiasMessage_DatabaseList_GivesTheGroupNamesAlone()
    {
        BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage([Idol("karina", "aespa")], "", "head", DiscordIds.User, false);

        Assert.Equal("aespa", Assert.Single(Assert.Single(Menus(result)).Options).Value);
    }

    //A select menu holds 25 options, and a message 5 rows
    [Theory]
    [InlineData(30, new[] { 25, 5 })]
    [InlineData(125, new[] { 25, 25, 25, 25, 25 })]
    public void BuildBiasMessage_ManyGroups_SplitsThemIntoMenusOfTwentyFive(int groupCount, int[] menuSizes)
    {
        BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage(Groups(groupCount), "", "head", DiscordIds.User, false);

        Assert.Equal(menuSizes, Menus(result).Select(menu => menu.Options.Count));
        Assert.Equal(["BiasMenu_1", "BiasMenu_2"], Menus(result).Take(2).Select(menu => menu.CustomId));
        Assert.Equal("head", result.Message);
    }

    //It threw until 2026-11-23, failing the whole list
    [Fact]
    public void BuildBiasMessage_MoreGroupsThanFiveMenusHold_ShowsTheFirstAndCountsTheRest()
    {
        BiasMessageResult result = BiasListMessageProcessor.BuildBiasMessage(Groups(130), "", "head", DiscordIds.User, false);

        Assert.Equal(5, Menus(result).Count);
        Assert.Equal("GROUP 125", Menus(result)[^1].Options.Last().Label);
        Assert.Equal("head\n5 more not shown, give a group's name to see its members", result.Message);
    }

    [Fact]
    public void GroupMemberMessage_UsersGroup_LeavesTheUsersIdOut()
    {
        string message = BiasListGroupMemberMessageProcessor.CreateMessage([$"blackpink><{DiscordIds.User}"], [Idol("jisoo", "blackpink"), Idol("jennie", "blackpink")]);

        Assert.Equal("BLACKPINK:\n`JISOO`, `JENNIE`", message);
    }

    private static IdolResource Idol(string name, string group)
    {
        return new IdolResource { Name = name, GroupName = group };
    }

    private static List<IdolResource> Groups(int count)
    {
        return [.. Enumerable.Range(1, count).Select(i => Idol("member", $"group {i:000}"))];
    }

    private static List<SelectMenuComponent> Menus(BiasMessageResult result)
    {
        return [.. result.Component!.Components.Cast<ActionRowComponent>().SelectMany(row => row.Components).Cast<SelectMenuComponent>()];
    }
}
