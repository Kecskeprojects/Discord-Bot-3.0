using Discord_Bot.Enums;
using System;

namespace Discord_Bot.Communication;
public class TwitterContent(Uri url, TwitterContentTypeEnum type)
{
    public Uri Url { get; set; } = url;
    public TwitterContentTypeEnum Type { get; set; } = type;
}
