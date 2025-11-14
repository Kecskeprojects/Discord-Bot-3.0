using Discord_Bot.Resources;

namespace Discord_Bot.Communication;

public class TwitchNotificationData
{
    public TwitchChannelResource TwitchChannel { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Title { get; set; }
}
