#nullable enable

using Discord_Bot.Resources;

namespace Discord_Bot.Communication;

public class TwitchNotificationData
{
    public required TwitchChannelResource TwitchChannel { get; set; }
    public required string ThumbnailUrl { get; set; }
    public required string Title { get; set; }
}
