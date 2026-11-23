using DatabaseORM.Resources;
using Discord_Bot.Processors.MessageProcessor;

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
}
