#nullable enable

using Discord;

namespace Discord_Bot.Communication.Bias;

public class BiasMessageResult
{
    public required string Message { get; set; }
    public MessageComponent? Component { get; set; }
}
