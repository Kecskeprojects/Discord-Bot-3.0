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

    //A server member; messages ping them with Mention
    public static IGuildUser GuildUser(ulong id = DiscordIds.User, ulong guildId = DiscordIds.Server, string username = TestValues.Username)
    {
        IGuildUser user = Substitute.For<IGuildUser>();
        user.Id.Returns(id);
        user.GuildId.Returns(guildId);
        user.Username.Returns(username);
        user.Mention.Returns($"<@{id}>");
        return user;
    }

    //A server the bot is in. Give it members and channels with GetUserAsync(id, CacheMode.CacheOnly).Returns(...) and the like
    public static IGuild Guild(ulong id = DiscordIds.Server, string name = TestValues.ServerName)
    {
        IGuild guild = Substitute.For<IGuild>();
        guild.Id.Returns(id);
        guild.Name.Returns(name);
        return guild;
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

    //A choice in a select menu with this custom ID; RespondWithModalAsync succeeds, check it with Received()
    public static IComponentInteraction SelectMenuChoice(string customId, string value, IUser user)
    {
        IComponentInteractionData data = Substitute.For<IComponentInteractionData>();
        data.CustomId.Returns(customId);
        data.Type.Returns(ComponentType.SelectMenu);
        data.Values.Returns([value]);

        IComponentInteraction choice = Substitute.For<IComponentInteraction>();
        choice.Data.Returns(data);
        choice.User.Returns(user);
        choice.Type.Returns(InteractionType.MessageComponent);
        return choice;
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
