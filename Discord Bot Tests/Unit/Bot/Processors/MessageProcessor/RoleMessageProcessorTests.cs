using DatabaseORM.Resources;
using Discord_Bot.Processors.MessageProcessor;
using Discord_Bot.Tests.Common.Tools;

namespace Discord_Bot.Tests.Unit.Bot.Processors.MessageProcessor;

//!update role message: the self-roles, as the role channel's pinned message shows them
public sealed class RoleMessageProcessorTests
{
    [Fact]
    public void CreateMessage_ListsTheRolesInTitleCaseAndInOrder()
    {
        List<RoleResource> roles = [new() { RoleName = "valorant" }, new() { RoleName = "league of legends" }, new() { RoleName = "among us" }];

        string message = RoleMessageProcessor.CreateMessage(roles);

        Assert.Equal(
            "Start the message with `+` or `-` to add/remove roles (roles are not case sensitive)\n\n" +
            "The current list of roles:\n" +
            "- `Among Us`\n- `League Of Legends`\n- `Valorant`",
            message);
    }

    //Hungarian sorts "cs" as a letter of its own after "c", so until 2026-11-24 the order followed the machine's culture
    [Theory]
    [InlineData("en-US")]
    [InlineData("hu-HU")]
    public void CreateMessage_InAnyCulture_OrdersTheRolesTheSame(string culture)
    {
        using CultureScope scope = new(culture);

        string message = RoleMessageProcessor.CreateMessage([new() { RoleName = "cube" }, new() { RoleName = "csgo" }]);

        Assert.EndsWith("- `Csgo`\n- `Cube`", message, StringComparison.Ordinal);
    }
}
