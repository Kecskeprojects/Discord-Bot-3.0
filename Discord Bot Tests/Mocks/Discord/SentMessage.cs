using Discord;

namespace Discord_Bot.Tests.Mocks.Discord;

//One message the bot sent to a channel (with the message it replies to, if any), and the message the channel gave back (to check what the bot did with it, such as pinning)
public sealed record SentMessage(string? Text, IReadOnlyList<Embed> Embeds, MessageComponent? Components, PollProperties? Poll, MessageReference? Reference, IUserMessage Message);
