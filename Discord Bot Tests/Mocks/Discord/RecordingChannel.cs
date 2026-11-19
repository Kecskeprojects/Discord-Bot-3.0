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
            Sent.Add(new SentMessage(call.ArgAt<string?>(0), embeds, args.OfType<MessageComponent>().FirstOrDefault()));

            IUserMessage message = Substitute.For<IUserMessage>();
            message.Id.Returns(DiscordIds.Message + (ulong) Sent.Count);
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
}
