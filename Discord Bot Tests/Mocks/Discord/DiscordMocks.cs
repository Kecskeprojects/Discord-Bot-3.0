using Discord;
using Discord.Commands;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Mocks.Discord;

//Substitutes for the Discord.Net interfaces the tests need most
public static class DiscordMocks
{
    public static IUser User(ulong id = DiscordIds.User, string username = TestValues.Username)
    {
        IUser user = Substitute.For<IUser>();
        user.Id.Returns(id);
        user.Username.Returns(username);
        user.Mention.Returns($"<@{id}>");
        user.GetDisplayAvatarUrl(default, default).ReturnsForAnyArgs(TestValues.AvatarUrl);
        return user;
    }

    //A text command in Discord.Net's own (non-socket) CommandContext, the kind CommandHandler makes from a gateway message;
    //run it with TestHost.RunCommandAsync
    public static ICommandContext TextCommand(string text, IUser author, IMessageChannel channel)
    {
        IUserMessage message = Substitute.For<IUserMessage>();
        message.Content.Returns(text);
        message.Author.Returns(author);
        message.Channel.Returns(channel);
        message.Id.Returns(DiscordIds.Message);
        return new CommandContext(Substitute.For<IDiscordClient>(), message);
    }

    //A click on a button with this custom ID. DeferAsync and ModifyOriginalResponseAsync succeed; check them with Received()
    public static IComponentInteraction ButtonClick(string customId, IUser user)
    {
        IComponentInteractionData data = Substitute.For<IComponentInteractionData>();
        data.CustomId.Returns(customId);
        data.Type.Returns(ComponentType.Button);
        data.Values.Returns([]);

        IComponentInteraction click = Substitute.For<IComponentInteraction>();
        click.Data.Returns(data);
        click.User.Returns(user);
        click.Type.Returns(InteractionType.MessageComponent);
        return click;
    }
}
