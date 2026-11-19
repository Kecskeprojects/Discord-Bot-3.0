using Discord;

namespace Discord_Bot.Tests.Mocks.Discord;

//One message the bot sent to a channel, and the message the channel gave back (to check what the bot did with it, such as pinning)
public sealed record SentMessage(string? Text, IReadOnlyList<Embed> Embeds, MessageComponent? Components, PollProperties? Poll, IUserMessage Message);
