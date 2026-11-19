using Discord;

namespace Discord_Bot.Tests.Mocks.Discord;

//One message the bot sent to a channel
public sealed record SentMessage(string? Text, IReadOnlyList<Embed> Embeds, MessageComponent? Components);
