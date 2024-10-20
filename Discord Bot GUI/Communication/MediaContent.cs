using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication;

public class MediaContent(Uri url, MediaContentTypeEnum type)
{
    public Uri Url { get; set; } = url;
    public MediaContentTypeEnum Type { get; set; } = type;
}
