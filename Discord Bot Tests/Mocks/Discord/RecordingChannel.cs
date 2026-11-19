using Discord;
using Discord_Bot.Tests.Common.Constants;

namespace Discord_Bot.Tests.Mocks.Discord;

/// <summary>
/// A channel substitute that keeps what the bot sends to it and which messages it deletes.
/// </summary>
public sealed class RecordingChannel
{
    private RecordingChannel(IMessageChannel channel, ulong id)
    {
        Channel = channel;
        channel.Id.Returns(id);

        //The arguments are read by type, so it doesn't matter which optional parameters the bot passed
        channel.SendMessageAsync().ReturnsForAnyArgs(call =>
        {
            object?[] args = call.Args();
            List<Embed> embeds = [.. args.OfType<Embed>(), .. args.OfType<Embed[]>().SelectMany(e => e)];
            IUserMessage message = Substitute.For<IUserMessage>();
            message.Id.Returns(DiscordIds.Message + (ulong) Sent.Count + 1);
            Sent.Add(new SentMessage(call.ArgAt<string?>(0), embeds, args.OfType<MessageComponent>().FirstOrDefault(), args.OfType<PollProperties>().FirstOrDefault(), message));
            return Task.FromResult(message);
        });
        channel.DeleteMessageAsync(default(ulong)).ReturnsForAnyArgs(call =>
        {
            Deleted.Add(call.ArgAt<ulong>(0));
            return Task.CompletedTask;
        });
    }

    public IMessageChannel Channel { get; }
    public List<SentMessage> Sent { get; } = [];
    public List<ulong> Deleted { get; } = [];

    public static RecordingChannel DirectMessage(ulong id = DiscordIds.DirectMessageChannel)
    {
        return new(Substitute.For<IDMChannel>(), id);
    }

    //A server's text channel; ITextChannel is also an IGuildChannel, so a guild can hand it out
    public static RecordingChannel Text(ulong id = DiscordIds.Channel)
    {
        return new(Substitute.For<ITextChannel>(), id);
    }

    //The user's direct messages: Discord.Net's IUser.SendMessageAsync extension opens them with CreateDMChannelAsync
    public static RecordingChannel DirectMessagesOf(IUser user)
    {
        RecordingChannel messages = DirectMessage();
        user.CreateDMChannelAsync(default).ReturnsForAnyArgs((IDMChannel) messages.Channel);
        return messages;
    }
}
